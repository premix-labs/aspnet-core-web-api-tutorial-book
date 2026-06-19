namespace Backend.Api.Exceptions;

public class ApiException : Exception
{
    public ApiException(string message, string code, int statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }

    public int StatusCode { get; }
}
