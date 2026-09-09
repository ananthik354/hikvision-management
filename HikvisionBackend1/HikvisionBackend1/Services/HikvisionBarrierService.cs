using System.Net;
using System.Text;

namespace HikvisionBackend1.Services
{
    public interface IHikvisionBarrierService
    {
        Task<bool> OpenBarrierAsync(
            string cameraIp,
            int httpPort,
            string username,
            string password,
            int channel = 1);

        Task<bool> CloseBarrierAsync(
            string cameraIp,
            int httpPort,
            string username,
            string password,
            int channel = 1);
    }

    public class HikvisionBarrierService : IHikvisionBarrierService
    {
        private readonly ILogger<HikvisionBarrierService> _logger;

        public HikvisionBarrierService(
            ILogger<HikvisionBarrierService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> OpenBarrierAsync(
            string cameraIp,
            int httpPort,
            string username,
            string password,
            int channel = 1)
        {
            return await SendBarrierCommandAsync(
                cameraIp,
                httpPort,
                username,
                password,
                channel,
                "open");
        }

        public async Task<bool> CloseBarrierAsync(
            string cameraIp,
            int httpPort,
            string username,
            string password,
            int channel = 1)
        {
            return await SendBarrierCommandAsync(
                cameraIp,
                httpPort,
                username,
                password,
                channel,
                "close");
        }

        private async Task<bool> SendBarrierCommandAsync(
            string cameraIp,
            int httpPort,
            string username,
            string password,
            int channel,
            string ctrlMode)
        {
            try
            {
                string url =
                    $"http://{cameraIp}:{httpPort}/ISAPI/Parking/channels/{channel}/barrierGate";

                string xml = $"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <BarrierGate>
                        <ctrlMode>{ctrlMode}</ctrlMode>
                    </BarrierGate>
                    """;

                var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(
                        username,
                        password
                    ),
                    PreAuthenticate = false
                };

                using var client = new HttpClient(handler);

                client.Timeout =
                    TimeSpan.FromSeconds(10);

                using var content =
                    new StringContent(
                        xml,
                        Encoding.UTF8,
                        "application/xml");

                _logger.LogInformation(
                    "Sending Hikvision barrier command: {Command} to {Url}",
                    ctrlMode,
                    url);

                using var response =
                    await client.PutAsync(
                        url,
                        content);

                string responseBody =
                    await response.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "Hikvision barrier response: HTTP {StatusCode} - {Response}",
                    (int)response.StatusCode,
                    responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Hikvision barrier command failed: {StatusCode}",
                        response.StatusCode);

                    return false;
                }

                return responseBody.Contains(
                    "<statusCode>1</statusCode>");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending Hikvision barrier command to {CameraIp}",
                    cameraIp);

                return false;
            }
        }
    }
}