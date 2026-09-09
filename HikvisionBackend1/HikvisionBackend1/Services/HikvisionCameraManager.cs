using HikvisionBackend1.Data;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Services
{
    public class HikvisionCameraManager
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HikvisionSdkService _sdkService;
        private readonly ILogger<HikvisionCameraManager> _logger;

        public HikvisionCameraManager(
            IServiceScopeFactory scopeFactory,
            HikvisionSdkService sdkService,
            ILogger<HikvisionCameraManager> logger)
        {
            _scopeFactory = scopeFactory;
            _sdkService = sdkService;
            _logger = logger;
        }

        public async Task ConnectAllCamerasAsync()
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var cameras = await db.Cameras
                .Where(c =>
                    c.IsapiEnabled &&
                    c.SdkEnabled)
                .ToListAsync();

            if (cameras.Count == 0)
            {
                _logger.LogWarning(
                    "No enabled Hikvision cameras found.");
                return;
            }

            _logger.LogInformation(
                "Found {Count} enabled Hikvision camera(s).",
                cameras.Count);

            foreach (var camera in cameras)
            {
                try
                {
                    _logger.LogInformation(
                        "Connecting Camera {CameraId} - {CameraName} - {IpAddress}",
                        camera.Id,
                        camera.Name,
                        camera.IpAddress);

                    bool loggedIn = _sdkService.Login(camera);

                    if (loggedIn)
                    {
                        _logger.LogInformation(
                            "Camera {CameraId} ({CameraName}) connected successfully.",
                            camera.Id,
                            camera.Name);

                        camera.Status = "Online";
                        camera.LastIsapiError = null;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Camera {CameraId} ({CameraName}) login failed.",
                            camera.Id,
                            camera.Name);

                        camera.Status = "Offline";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error connecting Camera {CameraId} ({CameraName}).",
                        camera.Id,
                        camera.Name);

                    camera.Status = "Offline";
                    camera.LastIsapiError = ex.Message;
                }
            }

            await db.SaveChangesAsync();
        }
    }
}