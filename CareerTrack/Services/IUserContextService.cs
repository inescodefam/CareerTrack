namespace CareerTrack.Services
{
    public interface IUserContextService
    {
        int GetCurrentUserId();
        string GetCurrentUsername();
    }
}
