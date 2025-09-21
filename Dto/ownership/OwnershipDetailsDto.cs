using TrakingCar.Dtos;

namespace TrakingCar.Dto.ownership
{
    public class OwnershipDetailsDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Detailes { get; set; }
        public string? LocationName { get; set; }
        public List<CarDto>? Cars { get; set; }
    }
}
