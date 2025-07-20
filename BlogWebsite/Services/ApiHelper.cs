using BlogWebsite.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlogWebsite.Services
{
    public class ApiHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpResponseMessage> SendWithAuthAsync(HttpMethod method, string endpoint, HttpContent? content = null)
        {
            var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var client = _httpClientFactory.CreateClient("BlogClient");

            var accessToken = context.Request.Cookies["accessToken"];
            var userId = context.Request.Cookies["userId"];
            var request = new HttpRequestMessage(method, endpoint);
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            if (content != null)
            {
                request.Content = content;
            }
            var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (context.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                {
                    var newTokens = await RefreshTokensAsync(client, refreshToken, userId);
                    if (newTokens != null)
                    {
                        SetAuthCookies(context, newTokens.Value.AccessToken, newTokens.Value.RefreshToken);
                        var retryRequest = new HttpRequestMessage(method, endpoint);
                        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.Value.AccessToken);
                        if (content != null)
                        {
                            var originalContent = await content.ReadAsStringAsync();
                            retryRequest.Content = new StringContent(originalContent, Encoding.UTF8, content.Headers.ContentType?.MediaType);
                        }
                        response = await client.SendAsync(retryRequest);
                    }
                    else
                    {
                        ClearAuthCookies(context);
                    }
                }
            }

            return response;
        }

        private async Task<(string AccessToken, string RefreshToken)?> RefreshTokensAsync(HttpClient client, string refreshToken, string userId)
        {
            try
            {
                TokenRequest refresh = new() { refreshToken = refreshToken, userId = Convert.ToInt32(userId) };

                var content = new StringContent(
                    JsonSerializer.Serialize(refresh),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("auth/refresh-token", content);

                if (response.IsSuccessStatusCode)
                {
                    using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                    return (
                        jsonDoc.RootElement.GetProperty("accessToken").GetString() ?? throw new InvalidOperationException("Access token missing"),
                        jsonDoc.RootElement.GetProperty("refreshToken").GetString() ?? throw new InvalidOperationException("Refresh token missing")
                    );
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        private void SetAuthCookies(HttpContext context, string accessToken, string refreshToken)
        {
            context.Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, 
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(10),
                Path = "/"
            });

            
            context.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            });
        }

        private void ClearAuthCookies(HttpContext context)
        {
            context.Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
            context.Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/" });
        }
    }
}