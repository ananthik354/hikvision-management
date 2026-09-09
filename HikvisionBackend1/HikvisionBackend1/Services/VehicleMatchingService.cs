using HikvisionBackend1.Data;
using HikvisionBackend1.Models;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Services
{
    public class VehicleMatchingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehicleMatchingService> _logger;

        private readonly VehicleTriggerService _triggerService;

        public VehicleMatchingService(
            AppDbContext context,
            ILogger<VehicleMatchingService> logger,
            VehicleTriggerService triggerService)
        {
            _context = context;
            _logger = logger;
            _triggerService = triggerService;
        }


        // ============================================================
        // CHECK VEHICLE MATCH
        // ============================================================

        public async Task<bool> CheckAndStoreMatchAsync(
            VehicleDetection detection)
        {
            try
            {
                // ========================================================
                // 1. CHECK PLATE NUMBER
                // ========================================================

                if (string.IsNullOrWhiteSpace(
                    detection.PlateNumber))
                {
                    _logger.LogInformation(
                        "No plate number for detection {Id}. No match.",
                        detection.Id);

                    return false;
                }


                string plateNumber =
                    detection.PlateNumber
                        .Trim()
                        .ToUpper();


                // ========================================================
                // 2. CHECK CAMERA ID
                // ========================================================

                if (detection.CameraId <= 0)
                {
                    _logger.LogWarning(
                        "Detection {DetectionId} has invalid CameraId.",
                        detection.Id);

                    return false;
                }


                // ========================================================
                // 3. FIND REGISTERED VEHICLE
                //
                // IMPORTANT:
                // CameraId + PlateNumber + Active
                //
                // This prevents:
                //
                // Front Camera registration
                // from matching
                // Back Camera detection.
                // ========================================================

                var registeredVehicle =
    await _context.RegisteredVehicles
        .FirstOrDefaultAsync(x =>
            x.CameraId == detection.CameraId &&
            x.PlateNumber.Trim().ToUpper() == plateNumber &&
            x.Status == "Active");


                // ========================================================
                // 4. NO MATCH
                // ========================================================

                if (registeredVehicle == null)
                {
                    _logger.LogInformation(
                        "NO MATCH. CameraId: {CameraId}, Plate: {PlateNumber}",
                        detection.CameraId,
                        plateNumber);

                    return false;
                }


                // ========================================================
                // 5. MATCH FOUND
                // ========================================================

                _logger.LogInformation(
                    "MATCH FOUND! CameraId: {CameraId}, Plate: {PlateNumber}, RegisteredVehicleId: {Id}, PassType: {PassType}",
                    detection.CameraId,
                    plateNumber,
                    registeredVehicle.Id,
                    registeredVehicle.PassType);


                // ========================================================
                // 6. CREATE VEHICLE HISTORY
                // ========================================================

                var history = new VehicleHistory
                {
                    RegisteredVehicleId =
                        registeredVehicle.Id,

                    VehicleDetectionId =
                        detection.Id,

                    PlateNumber =
                        detection.PlateNumber,

                    VehicleType =
                        detection.VehicleType,

                    FullVehicleImageBase64 =
                        detection.FullVehicleImageBase64,

                    PlateImageBase64 =
                        detection.PlateImageBase64,

                    DetectedAt =
                        detection.DetectedAt,

                    MatchedAt =
                        DateTime.Now,

                    MatchStatus =
                        "Matched",

                    Triggered = false
                };


                _context.VehicleHistory.Add(history);

                await _context.SaveChangesAsync();


                _logger.LogInformation(
                    "VehicleHistory saved. HistoryId: {HistoryId}",
                    history.Id);


                // ========================================================
                // 7. TRIGGER LIGHT / HIKVISION
                // ========================================================

                bool triggerSuccess =
     await _triggerService.TriggerAsync(
         detection.CameraId,
         registeredVehicle.Id,
         plateNumber);
                var camera =
    await _context.Cameras
        .AsNoTracking()
        .FirstOrDefaultAsync(
            c => c.Id == detection.CameraId);

                int relayOutput =
                    camera?.RelayOutput > 0
                        ? camera.RelayOutput
                        : 1;

                var relayAction = new RelayAction
                {
                    CameraId = detection.CameraId,

                    VehicleDetectionId = detection.Id,

                    RelayOutput = relayOutput,

                    Success = triggerSuccess,

                    Action = triggerSuccess
                        ? "OPEN"
                        : "OPEN_FAILED",

                    Message = triggerSuccess
                        ? $"Relay {relayOutput} opened successfully."
                        : $"Relay {relayOutput} failed to open.",

                    ExecutedAt = DateTime.Now
                };

                _context.RelayActions.Add(relayAction);

                await _context.SaveChangesAsync();

                // ========================================================
                // 8. UPDATE TRIGGER STATUS
                // ========================================================

                history.Triggered =
                    triggerSuccess;


                await _context.SaveChangesAsync();


                if (triggerSuccess)
                {
                    _logger.LogInformation(
                        "Light trigger SUCCESS. CameraId: {CameraId}, Plate: {PlateNumber}",
                        detection.CameraId,
                        plateNumber);
                }
                else
                {
                    _logger.LogWarning(
                        "Light trigger FAILED. CameraId: {CameraId}, Plate: {PlateNumber}",
                        detection.CameraId,
                        plateNumber);
                }


                // ========================================================
                // 9. ONE TIME PASS
                //
                // Only deactivate after successful trigger.
                // ========================================================

                if (registeredVehicle.PassType == "OneTime" &&
                    triggerSuccess)
                {
                    registeredVehicle.Status = "Inactive";

                    registeredVehicle.UsedAt =
                        DateTime.Now;

                    registeredVehicle.UpdatedAt =
                        DateTime.Now;


                    await _context.SaveChangesAsync();


                    _logger.LogInformation(
                        "ONE TIME PASS USED. RegisteredVehicleId: {Id}, Plate: {PlateNumber}. Status changed to Inactive.",
                        registeredVehicle.Id,
                        plateNumber);
                }


                // ========================================================
                // 10. ALL TIME PASS
                // ========================================================

                else if (registeredVehicle.PassType == "AllTime")
                {
                    _logger.LogInformation(
                        "ALL TIME PASS remains Active. RegisteredVehicleId: {Id}, Plate: {PlateNumber}",
                        registeredVehicle.Id,
                        plateNumber);
                }


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Vehicle matching failed for detection {DetectionId}",
                    detection.Id);

                throw;
            }
        }
    }
}