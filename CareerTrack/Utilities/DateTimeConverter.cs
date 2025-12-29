using CareerTrack.Models;

namespace CareerTrack.Utilities
{
    public class DateTimeConverter : IDateTimeConverter
    {
        public void ConvertToUtc(Goal goal)
        {
            goal.startDate = DateTime.SpecifyKind(goal.startDate, DateTimeKind.Utc);
            goal.targetDate = DateTime.SpecifyKind(goal.targetDate, DateTimeKind.Utc);
            if (goal.endDate.HasValue)
            {
                goal.endDate = DateTime.SpecifyKind(goal.endDate.Value, DateTimeKind.Utc);
            }
        }
    }
}
