using System;

namespace TrakingCar.Models
{
    public class LogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? UserName { get; set; }
        public string? IP { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public int StatusCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
