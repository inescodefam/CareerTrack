using CareerTrack.Models;

namespace CareerTrack.Services
{
    public interface IProgressService
    {
        void InitializeProgress(int goalId, int userId);
        GoalProgress? GetProgress(int goalId, int userId);
        void UpdateProgress(int goalId, int userId, int percentage, string? notes = null);
        IEnumerable<GoalProgress> GetProgressHistory(int goalId, int userId);

    }
}