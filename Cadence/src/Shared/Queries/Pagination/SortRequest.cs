namespace SharedKernel.Queries.Pagination;

public sealed record SortRequest(
    string Field,
    SortDirection Direction = SortDirection.Asc
);
