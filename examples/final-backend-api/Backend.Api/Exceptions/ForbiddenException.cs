namespace Backend.Api.Exceptions;

public class ForbiddenException(string message, string code = "FORBIDDEN")
    : ApiException(message, code, StatusCodes.Status403Forbidden);
