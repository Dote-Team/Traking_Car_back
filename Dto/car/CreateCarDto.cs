using Microsoft.AspNetCore.Http;

namespace TrakingCar.Dto.Car
{
    public class CreateCarDto
    {
        public string? CarType { get; set; }
        public string? ChassisNumber { get; set; }
        public string? CarNumber { get; set; }
        public string? Status { get; set; }
        public Guid? OwnershipId { get; set; }
        public Guid? LocationId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string? BodyCondition { get; set; }
        public string Note { get; set; }
        public string TrackingCode { get; set; }

        // ✅ قائمة المرفقات مع كامل البيانات
        public List<CreateAttachmentDto>? Attachments { get; set; }
    }

    public class CreateAttachmentDto
    {
        public string? FileName { get; set; }         
        public Guid? LocationId { get; set; }         
        public IFormFile? FileContent { get; set; }  
    }
}
