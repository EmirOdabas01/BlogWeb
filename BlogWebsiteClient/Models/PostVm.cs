using BlogWebsiteClient.Enums;

namespace BlogWebsiteClient.Models
{
    public class PostVm 
    {
        public int? id { get; set; }
        public string header { get; set; } = String.Empty;
        public PostType postCategory { get; set; }
        public DateTime createdAt { get; set; }
        public List<PostBlockVm> blocks { get; set; } = new();
    }
}
