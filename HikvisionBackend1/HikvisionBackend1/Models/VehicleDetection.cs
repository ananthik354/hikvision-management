using System;
using System.Collections.Generic;

namespace HikvisionBackend1.Models
{
    public class VehicleDetection
    {
        public int Id { get; set; }

        public int CameraId { get; set; }

        public string CameraName { get; set; } = string.Empty;

        public string? PlateNumber { get; set; }

        public string VehicleType { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? ImageFileName { get; set; }

        public DateTime DetectedAt { get; set; }

        public int? ConfidenceLevel { get; set; }

        public string? XmlFileName { get; set; }


        public string? FullVehicleImageBase64 { get; set; }

        public string? PlateImageBase64 { get; set; }

        public string? XmlData { get; set; }

        public string? Direction { get; set; }

        public string? Country { get; set; }

        public string? UUID { get; set; }
        public string? PlateBinaryImageBase64 { get; set; }

        // Navigation property
        public Camera? Camera { get; set; }
    }
}