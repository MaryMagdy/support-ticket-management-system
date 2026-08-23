namespace SupportTickets.Application.Common;

public class AppException : Exception
{
    public int StatusCode { get; }
    public AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message = "Resource not found") : base(message, 404) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action") : base(message, 403) { }
}

public class ValidationAppException : AppException
{
    public IDictionary<string, string[]>? Errors { get; }
    public ValidationAppException(string message, IDictionary<string, string[]>? errors = null) : base(message, 400)
    {
        Errors = errors;
    }
}

public class ConflictAppException : AppException
{
    public ConflictAppException(string message) : base(message, 409) { }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Invalid credentials") : base(message, 401) { }
}
