namespace TrakingCar.Dto.attachment
{
    public class UpdateAttachmentDto
    {
        public string? File { get; set; }
        public Guid? CarId { get; set; }
        public Guid? LocationId { get; set; }
    }
}
