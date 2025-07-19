namespace BlogWebsite.Services
{
    using System.Net;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;

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
            var context = _httpContextAccessor.HttpContext!;
            var client = _httpClientFactory.CreateClient("BlogClient");

            var accessToken = context.Session.GetString("AccessToken");

            var request = new HttpRequestMessage(method, endpoint);
            if (accessToken != null)
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
                if (context.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
                {
                    var refreshRequest = new
                    {
                        refreshToken = refreshToken
                    };

                    var refreshContent = new StringContent(
                        JsonSerializer.Serialize(refreshRequest),
                        Encoding.UTF8,
                        "application/json");

                    var refreshResponse = await client.PostAsync("auth/refresh-token", refreshContent);

                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        var json = await refreshResponse.Content.ReadAsStringAsync();
                        var result = JsonDocument.Parse(json).RootElement;
                        var newAccessToken = result.GetProperty("accessToken").GetString();

                        context.Session.SetString("AccessToken", newAccessToken!);

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                        response = await client.SendAsync(request);
                    }
                    else
                    {
                        context.Session.Remove("AccessToken");
                        context.Response.Cookies.Delete("RefreshToken");
                    }
                }
            }

            return response;
        }
    }

}
