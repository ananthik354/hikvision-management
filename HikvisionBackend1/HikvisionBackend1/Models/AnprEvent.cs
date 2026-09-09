namespace HikvisionBackend1.Models
{
    public class AnprEvent
    {
        public int Id { get; set; }
        public int CameraId { get; set; }
        public string? CameraName { get; set; }
        public string? PlateNumber { get; set; }
        public string? VehicleType { get; set; }

        public string? ImageUrl { get; set; }
        public DateTime DetectedAt { get; set; }
    }
}
