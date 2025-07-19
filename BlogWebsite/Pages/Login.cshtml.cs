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

            var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("auth/admin-login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewData["error"] = "Giriþ baþarýsýz.";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(json).RootElement;

            var accessToken = result.GetProperty("accessToken").GetString();
            var refreshToken = result.GetProperty("refreshToken").GetString();

            HttpContext.Session.SetString("AccessToken", accessToken!);

            Response.Cookies.Append("RefreshToken", refreshToken!,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

            return RedirectToPage("/Index");
        }
    }
}
