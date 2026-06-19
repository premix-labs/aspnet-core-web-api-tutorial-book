using Microsoft.AspNetCore.Http;

namespace Backend.Api.Exceptions;

public class ForbiddenException(string message, string code)
    : ApiException(message, code, StatusCodes.Status403Forbidden);
