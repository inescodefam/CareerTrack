using CareerTrack.Interfaces;

namespace CareerTrack.Decorators
{
    public class NotificationDecorator : GoalDecorator
    {
        public NotificationDecorator(IGoalNotification goal) : base(goal)
        { }

        public override string GetDescription() => base.GetDescription() + " + Status Notification";

        public override void StatusNotification()
        {
            base.StatusNotification();
            Console.WriteLine($"[Notification] Sending status update!");
        }
    }
}
