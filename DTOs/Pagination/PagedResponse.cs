namespace ExpenseVista.API.DTOs.Pagination
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Page { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / RecordsPerPage);

        public PagedResponse(IEnumerable<T> data, int page, int recordsPerPage, int totalRecords)
        {
            Data = data;
            Page = page;
            RecordsPerPage = recordsPerPage;
            TotalRecords = totalRecords;
        }
    }

}
