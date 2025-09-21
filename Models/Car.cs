using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace TrakingCar.Models
{
    // ------------------------------------------------------
    // Car Table
    // ------------------------------------------------------
    [Table("Car")]
    public class Car
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("UniqueID")]
        public Guid Id { get; set; }

        [ForeignKey("Location")]
        [Column("locationId")]
        public Guid? LocationId { get; set; }
        public Location? Location { get; set; }

        [Column("carType")]
        public string? CarType { get; set; }

        [Column("ChassisNumber")]
        public string? ChassisNumber { get; set; }

        [Column("carNumber")]
        public string? CarNumber { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [ForeignKey("Ownership")]
        [Column("ownershipId")]
        public Guid? OwnershipId { get; set; }
        public Ownership? Ownership { get; set; }

        [Column("receiptDate")]
        public DateTime? ReceiptDate { get; set; }

        [Column("bodyCondition")]
        public string? BodyCondition { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        [Column("trackingCode")]
        public string? TrackingCode { get; set; }

        public DateTime? DeletedAt { get; set; }

        //[Required]
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        //[Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 🔗 علاقات
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
