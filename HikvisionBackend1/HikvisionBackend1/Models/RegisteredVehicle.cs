using System.Text.Json.Serialization;
namespace HikvisionBackend1.Models

{
    public class RegisteredVehicle
    {
        public int Id { get; set; }

        // Camera/Gate this vehicle is registered for
        public int CameraId { get; set; }

        // Bike / Car / Bus / Truck
        public string? VehicleType { get; set; }

        // Vehicle number plate
        public string PlateNumber { get; set; } = string.Empty;

        // OneTime / AllTime
        public string PassType { get; set; } = "OneTime";

        // Pending / Active / Inactive
        public string Status { get; set; } = "Pending";

        // Date and time of registration
        public DateTime EntryDateTime { get; set; }

        // Date/time when OneTime pass was used
        public DateTime? UsedAt { get; set; }

        // Existing fields
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Relationship with Camera
        [JsonIgnore]
        public Camera? Camera { get; set; }
    }
}