using HikvisionBackend1.Data;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Services
{
    public class HikvisionCameraConnectionWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HikvisionSdkService _sdkService;
        private readonly ILogger<HikvisionCameraConnectionWorker> _logger;

        public HikvisionCameraConnectionWorker(
            IServiceScopeFactory scopeFactory,
            HikvisionSdkService sdkService,
            ILogger<HikvisionCameraConnectionWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _sdkService = sdkService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Hikvision camera connection worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndConnectCamerasAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while checking Hikvision cameras.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }

        private async Task CheckAndConnectCamerasAsync()
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var cameras = await db.Cameras
                .Where(c =>
                    c.IsapiEnabled &&
                    c.SdkEnabled)
                .ToListAsync();

            foreach (var camera in cameras)
            {
                if (_sdkService.TryGetUserId(
                    camera.Id,
                    out _))
                {
                    continue;
                }

                _logger.LogInformation(
                    "Camera {CameraId} ({CameraName}) is not connected. Attempting reconnect.",
                    camera.Id,
                    camera.Name);

                bool connected = _sdkService.Login(camera);

                camera.Status = connected
                    ? "Online"
                    : "Offline";

                if (connected)
                {
                    camera.LastIsapiError = null;

                    _logger.LogInformation(
                        "Camera {CameraId} connected successfully.",
                        camera.Id);
                }
                else
                {
                    _logger.LogWarning(
                        "Camera {CameraId} connection failed.",
                        camera.Id);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}