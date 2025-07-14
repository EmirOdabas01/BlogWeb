using BlogWebsiteClient.Enums;

namespace BlogWebsiteClient.Models
{
    public class PostBlockVm
    {
        public int? id { get; set; }
        public int postId { get; set; }
        public BlockType blockCategory { get; set; }
        public string? content { get; set; }
        public string? imageUrl { get; set; }
        public int order { get; set; }
    }
}
