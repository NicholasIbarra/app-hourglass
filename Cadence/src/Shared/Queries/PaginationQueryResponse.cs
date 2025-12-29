namespace SharedKernel.Queries;

public record PaginationQueryResponse<T>(IEnumerable<T> Items, int TotalItems);