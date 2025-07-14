using BlogWebsiteClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace BlogWebsiteClient.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient httpClient;

        public PostVm? Post { get; set; }  

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient("BlogClient");
        }

        public async Task OnGetAsync(int id)
        {
            try
            {
                var response = await httpClient.GetAsync($"get-post/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    ViewData["error"] = $"API Hatasý: {response.StatusCode}";
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();

                Post = JsonSerializer.Deserialize<PostVm>(json);
            }
            catch (Exception ex)
            {
                ViewData["error"] = $"Bir hata oluþtu: {ex.Message}";
            }
        }
    }
}
