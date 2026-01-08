using CareerTrack.Interfaces;

namespace CareerTrack.Decorators
{
    public class ReminderDecorator : GoalDecorator
    {
        public ReminderDecorator(IGoalNotification goalDecorator) : base(goalDecorator)
        { }
        public override string GetDescription() => $" [Reminder Enabled] {DateTime.UtcNow} for: " + base.GetDescription();

        public override void SendReminder() => Console.WriteLine("Sending reminder..."); 
        // dalje slanje na google kalednar, email, sms i sl.

    }
}
