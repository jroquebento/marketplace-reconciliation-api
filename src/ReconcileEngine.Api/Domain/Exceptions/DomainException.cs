namespace ReconcileEngine.Api.Domain.Exceptions;

public class DomainException : Exception
{
    public int StatusCode { get; }
    public DomainException(string message, int statusCode = 409) : base(message)
    {
        StatusCode = statusCode;
    }
}
