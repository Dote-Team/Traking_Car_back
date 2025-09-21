namespace TrackingCar.Dto.user
{
    public class UserReadDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Number { get; set; }
        public bool? Statuse { get; set; }
        public string Role { get; set; }
    }
}
