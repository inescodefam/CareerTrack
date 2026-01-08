using CareerTrack.Models;

namespace CareerTrack.Interfaces
{
    public interface IGoalFactory
    {
        Goal CreateGoal(string goalType, string name, DateTime targetDate);
    }
}
