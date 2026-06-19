namespace Backend.Api.Exceptions;

public class ConflictException(string message, string code = "CONFLICT")
    : ApiException(message, code, StatusCodes.Status409Conflict);
