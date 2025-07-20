namespace BlogWebsite.Models
{
    public class TokenResponse
    {
        public int userId { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
