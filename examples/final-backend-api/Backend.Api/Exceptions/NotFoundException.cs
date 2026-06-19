namespace Backend.Api.Exceptions;

public class NotFoundException(string message, string code = "NOT_FOUND")
    : ApiException(message, code, StatusCodes.Status404NotFound);
