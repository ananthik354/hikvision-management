using System;
using System.Collections.Generic;

namespace HikvisionBackend1.Models
{
    public class Camera
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;


        // ==========================================
        // FTP - keep for now
        // ==========================================

        public string? FtpHost { get; set; }

        public int? FtpPort { get; set; }

        public string? FtpUsername { get; set; }

        public string? FtpPassword { get; set; }

        public string? FtpFolder { get; set; }


        // ==========================================
        // ISAPI
        // ==========================================

        public bool IsapiEnabled { get; set; } = true;

        public int HttpPort { get; set; } = 80;

        public int IsapiChannel { get; set; } = 1;


        // ==========================================
        // Hikvision SDK
        // ==========================================

        public bool SdkEnabled { get; set; } = true;

        public int SdkPort { get; set; } = 8000;

        public string? HikvisionUsername { get; set; }

        public string? HikvisionPassword { get; set; }


        // ==========================================
        // Relay
        // ==========================================

        public bool RelayEnabled { get; set; } = true;

        public int RelayOutput { get; set; } = 1;


        // ==========================================
        // ISAPI Status
        // ==========================================

        public bool IsIsapiOnline { get; set; } = false;

        public DateTime? LastIsapiReceivedAt { get; set; }

        public string? LastIsapiEventType { get; set; }

        public string? LastIsapiPlateNumber { get; set; }

        public string? LastIsapiError { get; set; }


        // ==========================================
        // Vehicle detections
        // ==========================================

        public ICollection<VehicleDetection> VehicleDetections
        {
            get;
            set;
        } = new List<VehicleDetection>();
    }
}