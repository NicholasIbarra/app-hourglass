using SharedKernel.Queries.Pagination;

namespace SharedKernel.Queries;

public record PaginationQuery
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; }

    public string? SortBy { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
