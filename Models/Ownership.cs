using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TrakingCar.Models
{
    // ------------------------------------------------------
    // Ownership Table
    // ------------------------------------------------------
    [Table("ownership")]
    public class Ownership
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("UniqueID")]
        public Guid Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("detailes")]
        public string? Detailes { get; set; }

        [Column("location")]
        public string? LocationName { get; set; }

        [ForeignKey("Location")]
        [Column("locationId")]
        public Guid? LocationId { get; set; }
        public Location? Location { get; set; }

        public DateTime? DeletedAt { get; set; }

        //[Required]
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        //[Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 🔗 علاقات
        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
