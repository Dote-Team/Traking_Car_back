namespace TrakingCar.Dto.attachment
{
    public class AttachmentDetailsDto
    {
        public Guid Id { get; set; }
        public string? File { get; set; }
        public Guid? CarId { get; set; }
        public string? CarNumber { get; set; }

        public Guid? LocationId { get; set; }
        public string? LocationName { get; set; }
    }
}
