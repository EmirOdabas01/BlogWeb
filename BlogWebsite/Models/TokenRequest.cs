namespace BlogWebsite.Models
{
    public class TokenRequest
    {
        public required int userId { get; set; }
        public required string refreshToken { get; set; }
    }
}
