using BlogWebsiteClient.Enums;
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

        public async Task OnGetAsync(string idOrCategory)
        {
            HttpResponseMessage response;
            try
            {
                if (idOrCategory == "about")
                    response = await httpClient.GetAsync($"post/get-category/{PostType.About}");
                else
                    response = await httpClient.GetAsync("post/get/" + idOrCategory);

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
