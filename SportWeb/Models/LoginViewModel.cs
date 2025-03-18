namespace SportWeb.Models
{
    public class LoginViewModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? SiteKey { get; set; }
        public string? ReCaptchaToken { get; set; }
    }
}
