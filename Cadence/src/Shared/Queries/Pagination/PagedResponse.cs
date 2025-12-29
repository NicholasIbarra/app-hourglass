namespace SharedKernel.Queries.Pagination;

public sealed record PagedResponse<T>(
    IEnumerable<T> Items,
    PaginationInfo Pagination
);

