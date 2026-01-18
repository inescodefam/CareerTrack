using CareerTrack.Interfaces;

namespace CareerTrack.Decorators
{
    public abstract class GoalDecorator : IGoalNotification
    {
        private readonly IGoalNotification _goalDecorator;

        protected GoalDecorator(IGoalNotification goalDecorator)
        {
            _goalDecorator = goalDecorator;
        }
        public virtual string GetDescription() => _goalDecorator.GetDescription();
        public virtual void SendReminder() => _goalDecorator.SendReminder();
        public virtual void StatusNotification() => _goalDecorator.StatusNotification();
    }
}
