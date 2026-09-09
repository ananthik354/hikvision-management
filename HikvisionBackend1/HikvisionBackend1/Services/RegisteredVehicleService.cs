using HikvisionBackend1.Data;
using HikvisionBackend1.Models;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Services
{
    public class RegisteredVehicleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RegisteredVehicleService> _logger;

        public RegisteredVehicleService(
            AppDbContext context,
            ILogger<RegisteredVehicleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================================
        // GET ALL
        // ============================================================

        public async Task<List<RegisteredVehicle>> GetAllAsync()
        {
            return await _context.RegisteredVehicles
                .AsNoTracking()
                .OrderByDescending(x => x.EntryDateTime)
                .ToListAsync();
        }


        // ============================================================
        // GET BY ID
        // ============================================================

        public async Task<RegisteredVehicle?> GetByIdAsync(int id)
        {
            return await _context.RegisteredVehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }


        // ============================================================
        // GET BY CAMERA
        // Front Gate / Back Gate
        // ============================================================

        public async Task<List<RegisteredVehicle>> GetByCameraIdAsync(
            int cameraId)
        {
            return await _context.RegisteredVehicles
                .AsNoTracking()
                .Where(x => x.CameraId == cameraId)
                .OrderByDescending(x => x.EntryDateTime)
                .ToListAsync();
        }


        // ============================================================
        // CREATE
        // ============================================================

        public async Task<RegisteredVehicle> CreateAsync(
            RegisteredVehicle vehicle)
        {
            // Validate Camera
            var camera =
                await _context.Cameras
                    .FirstOrDefaultAsync(x =>
                        x.Id == vehicle.CameraId);

            if (camera == null)
            {
                throw new Exception(
                    $"Camera with ID {vehicle.CameraId} not found.");
            }


            // Clean plate number
            vehicle.PlateNumber =
                vehicle.PlateNumber
                    .Trim()
                    .ToUpper();


            // Default VehicleType
            if (string.IsNullOrWhiteSpace(vehicle.VehicleType))
            {
                vehicle.VehicleType = "Unknown";
            }


            // Only allow valid pass types
            if (vehicle.PassType != "OneTime" &&
                vehicle.PassType != "AllTime")
            {
                vehicle.PassType = "OneTime";
            }


            // New registration starts as Pending
            vehicle.Status = "Pending";


            // Registration date/time
            if (vehicle.EntryDateTime == default)
            {
                vehicle.EntryDateTime = DateTime.Now;
            }


            vehicle.UsedAt = null;

            vehicle.CreatedAt = DateTime.Now;

            vehicle.UpdatedAt = null;


            _context.RegisteredVehicles.Add(vehicle);

            await _context.SaveChangesAsync();


            _logger.LogInformation(
                "Registered vehicle added. ID: {Id}, CameraId: {CameraId}, Plate: {Plate}, PassType: {PassType}",
                vehicle.Id,
                vehicle.CameraId,
                vehicle.PlateNumber,
                vehicle.PassType);


            return vehicle;
        }


        // ============================================================
        // UPDATE
        // ============================================================

        public async Task<bool> UpdateAsync(
            int id,
            RegisteredVehicle updatedVehicle)
        {
            var vehicle =
                await _context.RegisteredVehicles
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (vehicle == null)
            {
                return false;
            }


            // Validate Camera
            var camera =
                await _context.Cameras
                    .FirstOrDefaultAsync(x =>
                        x.Id == updatedVehicle.CameraId);

            if (camera == null)
            {
                throw new Exception(
                    $"Camera with ID {updatedVehicle.CameraId} not found.");
            }


            vehicle.CameraId =
                updatedVehicle.CameraId;


            vehicle.PlateNumber =
                updatedVehicle.PlateNumber
                    .Trim()
                    .ToUpper();


            vehicle.VehicleType =
                updatedVehicle.VehicleType;


            // Validate PassType
            if (updatedVehicle.PassType == "OneTime" ||
                updatedVehicle.PassType == "AllTime")
            {
                vehicle.PassType =
                    updatedVehicle.PassType;
            }


            // Validate Status
            if (updatedVehicle.Status == "Pending" ||
                updatedVehicle.Status == "Active" ||
                updatedVehicle.Status == "Inactive")
            {
                vehicle.Status =
                    updatedVehicle.Status;
            }


            vehicle.EntryDateTime =
                updatedVehicle.EntryDateTime;


            // Don't allow accidental UsedAt changes
            // from normal update
            if (vehicle.Status != "Inactive")
            {
                vehicle.UsedAt = null;
            }


            vehicle.UpdatedAt = DateTime.Now;


            await _context.SaveChangesAsync();


            _logger.LogInformation(
                "Registered vehicle updated. ID: {Id}, CameraId: {CameraId}, Plate: {Plate}",
                vehicle.Id,
                vehicle.CameraId,
                vehicle.PlateNumber);


            return true;
        }


        // ============================================================
        // ACTIVATE VEHICLE
        // Pending → Active
        // ============================================================

        public async Task<bool> ActivateAsync(int id)
        {
            var vehicle =
                await _context.RegisteredVehicles
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (vehicle == null)
            {
                return false;
            }


            vehicle.Status = "Active";
            vehicle.UpdatedAt = DateTime.Now;


            await _context.SaveChangesAsync();


            _logger.LogInformation(
                "Registered vehicle activated. ID: {Id}, Plate: {Plate}",
                vehicle.Id,
                vehicle.PlateNumber);


            return true;
        }


        // ============================================================
        // DEACTIVATE VEHICLE
        // Active → Inactive
        // ============================================================

        public async Task<bool> DeactivateAsync(int id)
        {
            var vehicle =
                await _context.RegisteredVehicles
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (vehicle == null)
            {
                return false;
            }


            vehicle.Status = "Inactive";
            vehicle.UpdatedAt = DateTime.Now;


            await _context.SaveChangesAsync();


            _logger.LogInformation(
                "Registered vehicle deactivated. ID: {Id}, Plate: {Plate}",
                vehicle.Id,
                vehicle.PlateNumber);


            return true;
        }


        // ============================================================
        // DELETE
        // Used for end-of-day cleanup of OneTime registrations
        // ============================================================

        public async Task<bool> DeleteAsync(int id)
        {
            var vehicle =
                await _context.RegisteredVehicles
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (vehicle == null)
            {
                return false;
            }


            _context.RegisteredVehicles.Remove(vehicle);

            await _context.SaveChangesAsync();


            _logger.LogInformation(
                "Registered vehicle deleted. ID: {Id}, Plate: {Plate}",
                vehicle.Id,
                vehicle.PlateNumber);


            return true;
        }
    }
}