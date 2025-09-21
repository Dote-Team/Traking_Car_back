using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrakingCar.Models;

namespace TrakingCar.Data
{
    // ------------------------------------------------------
    // User Table
    // ------------------------------------------------------
    public enum UserRole
    {
        User = 1,
        Manager = 2,
        Admin = 3
    }

    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("UniqueID")]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        [Column("name")]
        public string? UserName { get; set; }

        [Required, MinLength(6)]
        [Column("password")]
        public string? Password { get; set; }

        [Required, MaxLength(100)]
        [Column("fullname")]
        public string? FullName { get; set; }

        [Column("number")]
        public string? Number { get; set; }

        [Column("statuse")]
        public bool? Statuse { get; set; } = true;

        [Column("role")]
        public UserRole? Role { get; set; }

        [Column("image")]
        public string? Image { get; set; }

        [Column("R_Token")]
        [MaxLength(255)]
        public string? RToken { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        //[Required]
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        //[Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}