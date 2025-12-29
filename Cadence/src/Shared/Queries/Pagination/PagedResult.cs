namespace SharedKernel.Queries.Pagination;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    PaginationInfo Pagination
);

