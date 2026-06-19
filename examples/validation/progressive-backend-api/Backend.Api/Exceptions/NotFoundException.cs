using Microsoft.AspNetCore.Http;

namespace Backend.Api.Exceptions;

public class NotFoundException(string message, string code)
    : ApiException(message, code, StatusCodes.Status404NotFound);
