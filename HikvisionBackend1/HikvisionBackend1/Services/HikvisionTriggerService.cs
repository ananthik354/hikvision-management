using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace HikvisionBackend1.Services
{
    public class HikvisionSettings
    {
        public string CameraIp { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string TriggerUrl { get; set; } = string.Empty;
        public bool UseHttps { get; set; }
    }


    public class HikvisionTriggerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HikvisionSettings _settings;
        private readonly ILogger<HikvisionTriggerService> _logger;

        public HikvisionTriggerService(
            IHttpClientFactory httpClientFactory,
            IOptions<HikvisionSettings> settings,
            ILogger<HikvisionTriggerService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }


        public async Task<(bool Success, string Message, string? Response)>
            TriggerCameraAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.CameraIp))
                {
                    return (
                        false,
                        "Hikvision CameraIp is not configured.",
                        null);
                }

                if (string.IsNullOrWhiteSpace(_settings.TriggerUrl))
                {
                    return (
                        false,
                        "Hikvision TriggerUrl is not configured.",
                        null);
                }

                if (string.IsNullOrWhiteSpace(_settings.Username) ||
                    string.IsNullOrWhiteSpace(_settings.Password))
                {
                    return (
                        false,
                        "Hikvision username/password is not configured.",
                        null);
                }


                // ----------------------------------------------------
                // BUILD CAMERA URL
                // ----------------------------------------------------

                string scheme =
                    _settings.UseHttps
                        ? "https"
                        : "http";


                string url =
                    _settings.TriggerUrl;


                // Allow either a complete URL or just an API path
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url =
                        $"{scheme}://{_settings.CameraIp}{url}";
                }


                _logger.LogInformation(
                    "Hikvision trigger requested.");

                _logger.LogInformation(
                    "Camera IP: {CameraIp}",
                    _settings.CameraIp);

                _logger.LogInformation(
                    "Trigger URL: {TriggerUrl}",
                    url);


                // ----------------------------------------------------
                // HTTP CLIENT
                // ----------------------------------------------------

                var handler =
                    new HttpClientHandler
                    {
                        Credentials =
                            new NetworkCredential(
                                _settings.Username,
                                _settings.Password),

                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };


                using var client =
                    new HttpClient(handler);

                client.Timeout =
                    TimeSpan.FromSeconds(15);


                // ----------------------------------------------------
                // REQUEST
                // ----------------------------------------------------

                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Post,
                        url);


                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(
                        "application/xml"));


                // ----------------------------------------------------
                // SEND
                // ----------------------------------------------------

                using HttpResponseMessage response =
                    await client.SendAsync(request);


                string responseBody =
                    await response.Content.ReadAsStringAsync();


                _logger.LogInformation(
                    "Hikvision trigger response: {StatusCode}",
                    response.StatusCode);


                _logger.LogInformation(
                    "Hikvision response: {Response}",
                    responseBody);


                if (response.IsSuccessStatusCode)
                {
                    return (
                        true,
                        "Hikvision camera trigger successful.",
                        responseBody);
                }


                return (
                    false,
                    $"Hikvision camera returned HTTP {(int)response.StatusCode}.",
                    responseBody);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError(
                    "Hikvision trigger request timed out.");

                return (
                    false,
                    "Hikvision camera request timed out.",
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering Hikvision camera.");

                return (
                    false,
                    "Error communicating with Hikvision camera.",
                    ex.Message);
            }
        }
    }
}