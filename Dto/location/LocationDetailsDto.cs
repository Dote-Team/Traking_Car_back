using TrakingCar.Dtos;

namespace TrakingCar.Dto.location
{
    public class LocationDetailsDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Detailes { get; set; }
        public string? LocationName { get; set; }


        // قائمة السيارات المرتبطة
        public List<CarDto>? Cars { get; set; }

        // قائمة الملفات المرفقة
        public List<AttachmentDto>? Attachments { get; set; }
    }
}
