namespace HikvisionBackend1.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string Name { get; set; }= string.Empty;

        public string Email { get; set; }= string.Empty;

        public string PasswordHash { get; set; }= string.Empty;

        public int RoleId { get; set; } 

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}
