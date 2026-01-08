using CareerTrack.Models;

namespace CareerTrack.Repository
{
    public interface IGoalRepository
    {
        Goal Create(Goal goal);
        Goal Update(Goal goal);
        bool Delete(int id);
        Goal? GetById(int id);
        Goal? GetByIdAndUser(int goalId, int userId);
        IEnumerable<Goal> GetByUserId(int userId);
    }
}
