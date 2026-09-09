using HikvisionBackend1.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace HikvisionBackend1.Services
{
    public class VehicleTriggerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehicleTriggerService> _logger;

        public VehicleTriggerService(
            AppDbContext context,
            ILogger<VehicleTriggerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> TriggerAsync(
            int cameraId,
            int registeredVehicleId,
            string plateNumber)
        {
            try
            {
                // =====================================================
                // 1. VALIDATE CAMERA
                // =====================================================

                if (cameraId <= 0)
                {
                    _logger.LogError(
                        "Invalid CameraId: {CameraId}",
                        cameraId);

                    return false;
                }

                var camera = await _context.Cameras
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == cameraId);

                if (camera == null)
                {
                    _logger.LogError(
                        "Camera not found. CameraId: {CameraId}",
                        cameraId);

                    return false;
                }

                // =====================================================
                // 2. CHECK RELAY ENABLED
                // =====================================================

                if (!camera.RelayEnabled)
                {
                    _logger.LogWarning(
                        "Relay disabled for Camera {CameraId}. No trigger sent.",
                        cameraId);

                    return false;
                }

                // =====================================================
                // 3. VALIDATE CAMERA SETTINGS
                // =====================================================

                if (string.IsNullOrWhiteSpace(camera.IpAddress))
                {
                    _logger.LogError(
                        "Camera {CameraId} has no IP address.",
                        cameraId);

                    return false;
                }

                if (string.IsNullOrWhiteSpace(camera.HikvisionUsername))
                {
                    _logger.LogError(
                        "Camera {CameraId} has no username.",
                        cameraId);

                    return false;
                }

                if (string.IsNullOrWhiteSpace(camera.HikvisionPassword))
                {
                    _logger.LogError(
                        "Camera {CameraId} has no password.",
                        cameraId);

                    return false;
                }

                // =====================================================
                // 4. CAMERA SETTINGS
                // =====================================================

                int httpPort =
                    camera.HttpPort > 0
                        ? camera.HttpPort
                        : 80;

                int channel =
                    camera.IsapiChannel > 0
                        ? camera.IsapiChannel
                        : 1;

                int relayOutput =
                    camera.RelayOutput > 0
                        ? camera.RelayOutput
                        : 1;

                string cameraIp =
                    camera.IpAddress.Trim();

                string username =
                    camera.HikvisionUsername.Trim();

                string password =
                    camera.HikvisionPassword;

                // =====================================================
                // 5. LOG
                // =====================================================

                _logger.LogInformation(
                    "============================================");

                _logger.LogInformation(
                    "CAMERA-SPECIFIC RELAY TRIGGER");

                _logger.LogInformation(
                    "CameraId: {CameraId}",
                    cameraId);

                _logger.LogInformation(
                    "CameraName: {CameraName}",
                    camera.Name);

                _logger.LogInformation(
                    "CameraIP: {CameraIp}",
                    cameraIp);

                _logger.LogInformation(
                    "RelayOutput: {RelayOutput}",
                    relayOutput);

                _logger.LogInformation(
                    "Plate: {PlateNumber}",
                    plateNumber);

                _logger.LogInformation(
                    "RegisteredVehicleId: {RegisteredVehicleId}",
                    registeredVehicleId);

                // =====================================================
                // 6. HIKVISION ISAPI BARRIER URL
                // =====================================================

                string triggerUrl =
                    $"http://{cameraIp}:{httpPort}" +
                    $"/ISAPI/Parking/channels/{channel}/barrierGate";

                _logger.LogInformation(
                    "Trigger URL: {TriggerUrl}",
                    triggerUrl);

                // =====================================================
                // 7. HTTP CLIENT
                // =====================================================

                var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(
                        username,
                        password),

                    PreAuthenticate = false,

                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler
                            .DangerousAcceptAnyServerCertificateValidator
                };

                using var client =
                    new HttpClient(handler);

                client.Timeout =
                    TimeSpan.FromSeconds(10);

                // =====================================================
                // 8. OPEN BARRIER
                // =====================================================

                string xml = """
                    <?xml version="1.0" encoding="UTF-8"?>
                    <BarrierGate>
                        <ctrlMode>open</ctrlMode>
                    </BarrierGate>
                    """;

                using var content =
                    new StringContent(
                        xml,
                        Encoding.UTF8,
                        "application/xml");

                _logger.LogInformation(
                    "Sending relay OPEN command to Camera {CameraId}, RelayOutput {RelayOutput}.",
                    cameraId,
                    relayOutput);

                using HttpResponseMessage response =
                    await client.PutAsync(
                        triggerUrl,
                        content);

                string responseBody =
                    await response.Content
                        .ReadAsStringAsync();

                _logger.LogInformation(
                    "Hikvision HTTP Status: {StatusCode}",
                    (int)response.StatusCode);

                _logger.LogInformation(
                    "Hikvision Response: {Response}",
                    responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Relay trigger failed. CameraId: {CameraId}, HTTP: {StatusCode}",
                        cameraId,
                        (int)response.StatusCode);

                    return false;
                }

                // =====================================================
                // 9. CHECK HIKVISION RESPONSE
                // =====================================================

                bool success = false;

                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    try
                    {
                        XDocument responseXml =
                            XDocument.Parse(responseBody);

                        string? statusCode =
                            responseXml
                                .Descendants()
                                .FirstOrDefault(x =>
                                    x.Name.LocalName.Equals(
                                        "statusCode",
                                        StringComparison.OrdinalIgnoreCase))
                                ?.Value;

                        success =
                            statusCode == "1";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Could not parse Hikvision response.");
                    }
                }

                // =====================================================
                // 10. RESULT
                // =====================================================

                if (success)
                {
                    _logger.LogInformation(
                        "RELAY OPEN SUCCESS. CameraId: {CameraId}, RelayOutput: {RelayOutput}, Plate: {PlateNumber}",
                        cameraId,
                        relayOutput,
                        plateNumber);
                }
                else
                {
                    _logger.LogWarning(
                        "RELAY OPEN FAILED. CameraId: {CameraId}, RelayOutput: {RelayOutput}",
                        cameraId,
                        relayOutput);
                }

                _logger.LogInformation(
                    "============================================");

                return success;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError(
                    "Relay trigger timeout. CameraId: {CameraId}, Plate: {PlateNumber}",
                    cameraId,
                    plateNumber);

                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Could not connect to Camera {CameraId}.",
                    cameraId);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Relay trigger failed. CameraId: {CameraId}",
                    cameraId);

                return false;
            }
        }
    }
}