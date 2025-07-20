using BlogWebsite.Services;
using BlogWebsiteClient.Enums;
using BlogWebsiteClient.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;
namespace BlogWebsite.Pages
{
    public class AddPostModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApiHelper _apiHelper;
        public AddPostModel(IWebHostEnvironment env, ApiHelper apiHelper)
        {
            _env = env;
            _apiHelper = apiHelper;
        }


        [BindProperty]
        public string PostContent { get; set; }  

        [BindProperty]
        public string Header { get; set; }

        [BindProperty]
        public PostType PostCategory { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            if(!Request.Cookies.TryGetValue("accessToken", out var token) ||  String.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Index");
            }
         
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(PostContent);

            var blocks = new List<PostBlockVm>();
            int order = 0;

            foreach (var node in doc.DocumentNode.Descendants())
            {
                if (node.Name == "p" && !string.IsNullOrWhiteSpace(node.InnerText))
                {
                    blocks.Add(new PostBlockVm
                    {
                        blockCategory = BlockType.Text,
                        content = node.InnerText.Trim(),
                        order = order++
                    });
                }
                else if (node.Name == "img")
                {
                    var src = node.GetAttributeValue("src", "");
                    if (string.IsNullOrWhiteSpace(src))
                        continue;

                    string imageUrl;

                    if (src.StartsWith("data:image")) // base64 resim
                    {
                        var imageData = src.Split(',')[1]; // base64 kýsmý
                        var bytes = Convert.FromBase64String(imageData);

                        var ext = GetImageExtension(src);
                        var fileName = $"{Guid.NewGuid()}.{ext}";
                        var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                        System.IO.File.WriteAllBytes(filePath, bytes);

                        imageUrl = $"/uploads/{fileName}"; // web eriþimi için URL
                    }
                    else
                    {
                        // zaten URL varsa onu al
                        imageUrl = src;
                    }

                    blocks.Add(new PostBlockVm
                    {
                        blockCategory = BlockType.Image,
                        imageUrl = imageUrl,
                        order = order++
                    });
                }
            }

            var post = new PostVm
            {
                header = Header,
                postCategory = PostCategory,
                createdAt = DateTime.Now,
                blocks = blocks
            };

            var json = JsonSerializer.Serialize(post);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _apiHelper.SendWithAuthAsync(HttpMethod.Post, "post/create", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Index");
               
            }
            return Page();
        }
        private string GetImageExtension(string base64Src)
        {
            if (base64Src.Contains("image/jpeg")) return "jpg";
            if (base64Src.Contains("image/png")) return "png";
            if (base64Src.Contains("image/gif")) return "gif";
            return "jpg"; 
        }
    }
}
