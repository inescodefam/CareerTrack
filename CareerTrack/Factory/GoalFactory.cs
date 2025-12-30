using CareerTrack.Interfaces;
using CareerTrack.Models;

namespace CareerTrack.Factory
{
    public class GoalFactory : IGoalFactory
    {
        public Goal CreateGoal(string goalType, string name, DateTime targetDate)
        {
            var timespan = targetDate - DateTime.UtcNow;

            return goalType.ToLower() switch
            {
                "skill" => new SkillGoal
                {
                    Name = name,
                    targetDate = targetDate,
                    startDate = DateTime.UtcNow
                },
                "short" => new ShortTermGoal
                {
                    Name = name,
                    targetDate = targetDate,
                    startDate = DateTime.UtcNow,
                    ReminderFrequencyDays = 7
                },
                "long" => new LongTermGoal
                {
                    Name = name,
                    targetDate = targetDate,
                    startDate = DateTime.UtcNow,
                    Milestones = new List<string>()
                },
                _ => new Goal
                {
                    Name = name,
                    targetDate = targetDate,
                    startDate = DateTime.UtcNow
                }
            };
        }
    }
}
