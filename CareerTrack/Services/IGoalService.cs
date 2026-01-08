/// 1. SINGLE RESPONSIBILITY PRINCIPLE (SRP)

using CareerTrack.Models;

namespace CareerTrack.Services
{
    public interface IGoalService
    {
        IEnumerable<Goal> GetUserGoals(int userId);
        Goal? GetGoalById(int goalId, int userId);
        Goal CreateGoal(Goal goal, int userId);
        Goal UpdateGoal(Goal goal, int userId);
        bool DeleteGoal(int goalId, int userId);
    }
}
