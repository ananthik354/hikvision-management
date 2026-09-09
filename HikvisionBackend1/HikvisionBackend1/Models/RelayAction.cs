namespace HikvisionBackend1.Models
{
    public class RelayAction
    {
        public int Id { get; set; }

        // Camera whose relay was controlled
        public int CameraId { get; set; }

        // Vehicle detection that caused the relay action
        public int VehicleDetectionId { get; set; }

        // Relay output number on that camera
        public int RelayOutput { get; set; }

        public bool Success { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime ExecutedAt { get; set; }

        // Relationships
        public Camera? Camera { get; set; }

        public VehicleDetection? VehicleDetection { get; set; }
    }
}