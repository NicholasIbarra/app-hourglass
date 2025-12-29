namespace SharedKernel.Queries.Pagination;

public sealed record PaginationInfo(
    int PageNumber,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages =>
        (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => PageNumber < TotalPages;

    public bool HasPreviousPage => PageNumber > 1;
}

