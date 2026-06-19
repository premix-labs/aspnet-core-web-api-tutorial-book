using Microsoft.AspNetCore.Http;

namespace Backend.Api.Exceptions;

public class ConflictException(string message, string code)
    : ApiException(message, code, StatusCodes.Status409Conflict);
