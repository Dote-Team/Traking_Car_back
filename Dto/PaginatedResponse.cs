namespace Tracking.Dto
{
    public class PaginatedResponse<T>
    {
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<T> Data { get; set; }
        public object Metadata { get; internal set; }
    }

}
