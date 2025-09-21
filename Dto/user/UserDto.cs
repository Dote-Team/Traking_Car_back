
namespace TrackingCar.Dto.user
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Number { get; set; }
        public bool? Statuse { get; set; }
        public string? Role { get; set; } 
        public string? Image { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
