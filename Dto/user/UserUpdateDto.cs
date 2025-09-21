using TrakingCar.Data;

namespace TrackingCar.Dto.user
{
    public class UserUpdateDto
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string? Number { get; set; }
        public bool? Statuse { get; set; } = true;
        public UserRole Role { get; set; }
        public IFormFile? Image { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
