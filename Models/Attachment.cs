using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TrakingCar.Models
{
    // ------------------------------------------------------
    // Attachment Table
    // ------------------------------------------------------
    [Table("Attachment")]
    public class Attachment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("UniqueID")]
        public Guid Id { get; set; }

        [Column("file")]
        public string? File { get; set; }

        [ForeignKey("Location")]
        [Column("locationId")]
        public Guid? LocationId { get; set; }
        public Location? Location { get; set; }

        [ForeignKey("Car")]
        [Column("CarId")]
        public Guid? CarId { get; set; }
        public Car? Car { get; set; }

        public DateTime? DeletedAt { get; set; }

        //[Required]
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        //[Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
