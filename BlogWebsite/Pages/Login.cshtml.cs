using BlogWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace BlogWebsite.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public LoginModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public string UserName { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _clientFactory.CreateClient("BlogClient");

            var loginDto = new
            {
                userName = UserName,
                password = Password
            };

            HttpResponseMessage response;
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
                response = await client.PostAsync("auth/admin-login", content);

                if (!response.IsSuccessStatusCode)
                {
                    ViewData["error"] = "Giriþ baþarýsýz.";
                    return Page();
                }
            }   
            catch (Exception)
            {
                    ViewData["error"] = "Giriþ baþarýsýz.";
                    return Page();
            }

            var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
            SetTokens(tokens!.accessToken, tokens.refreshToken);

            Response.Cookies.Append("userId", tokens.userId.ToString(), new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            });

            return RedirectToPage("/Index");
        }

        private void SetTokens(string accessToken, string refreshToken)
        {
            Response.Cookies.Append("accessToken", accessToken, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(10),
                Path = "/"

            });

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            });

        }

    }
}
