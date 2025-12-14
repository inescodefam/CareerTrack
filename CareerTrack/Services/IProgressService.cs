namespace CareerTrack.Services
{
    public interface IProgressService
    {
        void InitializeProgress(int goalId, int userId);

        void UpdateProgress(int goalId, int userId, int percentage, string? notes = null);

    }
}