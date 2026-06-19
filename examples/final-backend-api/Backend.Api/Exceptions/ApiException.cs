namespace Backend.Api.Exceptions;

public abstract class ApiException(string message, string code, int statusCode) : Exception(message)
{
    public string Code { get; } = code;

    public int StatusCode { get; } = statusCode;
}
