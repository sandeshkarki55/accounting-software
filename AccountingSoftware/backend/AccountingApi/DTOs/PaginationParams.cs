namespace AccountingApi.DTOs
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageNumber = 1;
        private int _pageSize = 10;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value > 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(PageNumber), value, "Page number must be greater than 0.");
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value switch
            {
                < 1 => throw new ArgumentOutOfRangeException(nameof(PageSize), value, "Page size must be at least 1."),
                > MaxPageSize => throw new ArgumentOutOfRangeException(nameof(PageSize), value,
                    $"Page size cannot exceed {MaxPageSize}."),
                _ => value
            };
        }
    }
}