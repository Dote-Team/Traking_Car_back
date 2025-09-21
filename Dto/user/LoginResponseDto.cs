namespace TrackingCar.Dto.user
{
    public class LoginResponseDto
    {
        public Guid Id { get; set; }
        public string User { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string FullName { get; set; }
        public string? Number { get; set; }       
        public string? Image { get; set; }
        public bool? Statuse { get; set; }
        





    }

}
