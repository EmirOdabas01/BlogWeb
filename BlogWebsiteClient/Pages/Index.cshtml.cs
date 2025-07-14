using BlogWebsiteClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace BlogWebsiteClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient httpClient;
        public List<PostVm> Posts { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient("BlogClient");
        }

        public async Task OnGetAsync()
        {
            try
            {
                var response = await httpClient.GetAsync("get-all");

                if (!response.IsSuccessStatusCode)
                {
                    ViewData["error"] = $"API Hatası: {response.StatusCode}";
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<PostVm>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Posts = result ?? new List<PostVm>();
            }
            catch (Exception ex)
            {
                ViewData["error"] = "Sunucudan veri alınırken bir hata oluştu: " + ex.Message;
            }
        }
    }
}
