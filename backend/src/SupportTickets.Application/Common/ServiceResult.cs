namespace SupportTickets.Application.Common;

public enum ServiceErrorType
{
    None,
    NotFound,
    Forbidden,
    Validation,
    Conflict
}

public class ServiceResult<T>
{
    public bool Succeeded { get; private set; }
    public T? Data { get; private set; }
    public string? Error { get; private set; }
    public ServiceErrorType ErrorType { get; private set; } = ServiceErrorType.None;

    public static ServiceResult<T> Success(T data) => new() { Succeeded = true, Data = data };

    public static ServiceResult<T> Fail(string error, ServiceErrorType type) =>
        new() { Succeeded = false, Error = error, ErrorType = type };
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
