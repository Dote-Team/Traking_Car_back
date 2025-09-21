using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TrakingCar.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AttachmentType
    {
        [EnumMember(Value = "صورة تخويل")]
        Authorization,  
        [EnumMember(Value = "صورة سنوية")]
        Annual,         

        [EnumMember(Value = "صورة مستند")]
        Document        
    }
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

        [Column("type")]
        public string? Type { get; set; } = null!;

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
