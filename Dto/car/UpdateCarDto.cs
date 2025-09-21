namespace TrakingCar.Dto.car
{
    public class UpdateCarDto
    {
        public Guid? LocationId { get; set; }
        public string? CarType { get; set; }
        public string? ChassisNumber { get; set; }
        public string? CarNumber { get; set; }
        public string? Status { get; set; }
        public Guid? OwnershipId { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public string? BodyCondition { get; set; }
        public string? Note { get; set; }
        public string? TrackingCode { get; set; }

        public List<IFormFile>? AttachmentsFiles { get; set; }

    }
}
