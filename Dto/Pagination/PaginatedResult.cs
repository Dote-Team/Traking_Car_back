namespace Tracking.Dto.Pagination
{
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Data { get; set; }
        public PaginationData Metadata { get; set; }
    }
}
