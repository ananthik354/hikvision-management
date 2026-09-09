namespace HikvisionBackend1.Models
{
    public class RelayResult
    {
        public bool Success { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime ExecutedAt { get; set; }
    }
}
