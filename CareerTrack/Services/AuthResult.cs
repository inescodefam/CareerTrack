namespace CareerTrack.Services
{
    public record AuthResult(bool success, string? errorMessage, string? reirectUrl )
    {
    }
}
