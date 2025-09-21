namespace TrackingCar.Dto.user
{
    public class LogEntryReadDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? IP { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public int StatusCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
