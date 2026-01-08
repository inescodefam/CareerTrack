using CareerTrack.Models;

namespace CareerTrack.Interfaces
{
    // chain of responsibility pattern
    public interface IGoalHandler
    {
        IGoalHandler SetNext(IGoalHandler handler);
        GoalHandlerResult Handle(GoalRequest request);
    }
}
