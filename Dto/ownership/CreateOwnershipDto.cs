namespace TrakingCar.Dto.ownership
{
    public class CreateOwnershipDto
    {
        public string? Name { get; set; }
        public string? Detailes { get; set; }
        public string? LocationName { get; set; }
        public Guid? LocationId { get; set; }
    }
}
