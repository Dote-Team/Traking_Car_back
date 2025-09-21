

using System.ComponentModel.DataAnnotations;
using TrakingCar.Data;

namespace TrackingCar.Dto.user
{
    public class RegisterationRequestDto
    {

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50, ErrorMessage = "Username can't exceed 50 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(100, ErrorMessage = "Full Name can't exceed 100 characters")]
        public string FullName { get; set; }

        public string? Number { get; set; }
        public bool? Statuse { get; set; } = true;
        public UserRole Role { get; set; }
        public IFormFile? Image { get; set; }





    }
}
