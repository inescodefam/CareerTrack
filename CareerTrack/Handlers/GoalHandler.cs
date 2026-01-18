using CareerTrack.Interfaces;
using CareerTrack.Models;

namespace CareerTrack.Handlers
{
    public abstract class GoalHandler : IGoalHandler
    {
        private IGoalHandler? _nextHandler;

        public IGoalHandler SetNext(IGoalHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }

        public virtual GoalHandlerResult Handle(GoalRequest request)
        {
            if (_nextHandler != null)
            {
                return _nextHandler.Handle(request);
            }

            return new GoalHandlerResult { Success = true };
        }
    }
}
