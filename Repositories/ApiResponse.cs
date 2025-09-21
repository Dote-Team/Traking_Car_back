
namespace TrackingCar.Repositories
{
    internal class ApiResponse
    {
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}