using System.Collections.Generic;
using System.Linq; // Add this using directive
using System;

namespace AccountingApi.DTOs
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>(); // Initialize with empty collection
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
