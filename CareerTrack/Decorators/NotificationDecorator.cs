using CareerTrack.Interfaces;

namespace CareerTrack.Decorators
{
    public class NotificationDecorator : GoalDecorator
    {
        public NotificationDecorator(IGoalNotification goal) : base(goal)
        { }

        public override string GetDescription() => base.GetDescription() + " + StatusNotification";

        public override void StatusNotification()
        {
            base.StatusNotification();
            Console.WriteLine($"[NotificationDecorator] Sending status update notification!");
        }
    }
}
