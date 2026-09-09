namespace HikvisionBackend1.Models
{
    public class VehicleHistory
    {
        public int Id { get; set; }

        public int RegisteredVehicleId { get; set; }

        public int VehicleDetectionId { get; set; }

        public string PlateNumber { get; set; } = string.Empty;

        public string? VehicleType { get; set; }

        public string? FullVehicleImageBase64 { get; set; }

        public string? PlateImageBase64 { get; set; }

        public DateTime DetectedAt { get; set; }

        public DateTime MatchedAt { get; set; }

        public string MatchStatus { get; set; } = "Matched";

        public bool Triggered { get; set; }

        public RegisteredVehicle? RegisteredVehicle { get; set; }

        public VehicleDetection? VehicleDetection { get; set; }
    }
}