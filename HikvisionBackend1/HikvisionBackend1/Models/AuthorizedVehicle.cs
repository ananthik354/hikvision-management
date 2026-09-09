namespace HikvisionBackend1.Models
{
    public class AuthorizedVehicle
    {
        public int Id { get; set; }

        public string PlateNumber { get; set; } = string.Empty;

        public string? VehicleType { get; set; }

        public string? OwnerName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
