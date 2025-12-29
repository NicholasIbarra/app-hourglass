using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Queries.Pagination;

public sealed record PageRequest(
    int PageNumber = 1,
    int PageSize = 25,
    string? SortBy = null,
    SortDirection Direction = SortDirection.Asc
);
