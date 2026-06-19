using Microsoft.AspNetCore.Http;

namespace Backend.Api.Exceptions;

public class UnauthorizedException(string message, string code)
    : ApiException(message, code, StatusCodes.Status401Unauthorized);
