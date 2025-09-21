using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using TrakingCar.Data;

namespace TrakingCar.Models
{
    // ------------------------------------------------------
    // Location Table
    // ------------------------------------------------------
    [Table("location")]
    public class Location
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

        public DateTime? DeletedAt { get; set; }

        //[Required]
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        //[Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        // 🔗 علاقات
        public ICollection<Car> Cars { get; set; } = new List<Car>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
