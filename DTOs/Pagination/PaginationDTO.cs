namespace ExpenseVista.API.DTOs.Pagination
{
    public class PaginationDTO
    {
        private int recordsPerPage = 10;
        private int maxRecordsPerPage = 20;
        
        public int Page { get; set; } = 1;
        //checks if the incoming RPP is greater than the set maximum RPP
        //defaults to a record of 20 if its greater
        public int RecordsPerPage
        {
            get { return recordsPerPage; }
            set
            {
                recordsPerPage = (value > maxRecordsPerPage) ? maxRecordsPerPage : value;
            }
        }
    }
}
