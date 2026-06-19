namespace Backend.Api.Exceptions;

public class UnauthorizedException(string message, string code = "UNAUTHORIZED")
    : ApiException(message, code, StatusCodes.Status401Unauthorized);
