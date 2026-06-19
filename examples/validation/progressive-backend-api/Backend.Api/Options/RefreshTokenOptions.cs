namespace Backend.Api.Options;

public class RefreshTokenOptions
{
    public int ExpirationDays { get; set; } = 30;
}
