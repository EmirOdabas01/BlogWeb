using BlogWebsite.Services;
using BlogWebsiteClient.Enums;
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

        public async Task OnGetAsync(string category)
        {

            HttpResponseMessage response;
            try
            {
                if(category == "technology")
                     response = await httpClient.GetAsync($"post/get-category/{PostType.Technology}");
                else if(category == "general")
                      response = await httpClient.GetAsync($"post/get-category/{PostType.Blog}");
                else if(category == "project")
                      response = await httpClient.GetAsync($"post/get-category/{PostType.Project}");
                else
                    response = await httpClient.GetAsync("post/get-all");

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
