namespace Realtime.Exceptions;

public class ApplicationException : Exception
{
    public int StatusCode { get; }
    public Dictionary<string, object?> Errors { get; }

    public ApplicationException(int statusCode, Dictionary<string, object?> errors = null!, string? message = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}