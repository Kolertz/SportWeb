using Microsoft.Extensions.Options;
using SportWeb.Models;
using System.Text.Json;

namespace SportWeb.Services
{
    public interface IReCaptchaService
    {
        Task<bool> IsCaptchaValid(string token);
    }

    public class ReCaptchaService(IOptions<GoogleReCaptchaSettings> settings, HttpClient httpClient) : IReCaptchaService
    {
        private readonly GoogleReCaptchaSettings _settings = settings.Value;

        public async Task<bool> IsCaptchaValid(string token)
        {
            var response = await httpClient.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={_settings.SecretKey}&response={token}");
            var captchaResult = JsonSerializer.Deserialize<ReCaptchaResponse>(response);
            return captchaResult!.Success;
        }
    }

}
