using CareerTrack.Interfaces;
using CareerTrack.Services.ExporterData;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models
{
    public class Goal
    {
        [BindRequired]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, BindRequired]
        public DateTime targetDate { get; set; }

        [Required, BindRequired]
        public DateTime startDate { get; set; }

        public DateTime? endDate { get; set; }

        [Required, BindRequired]
        public int UserId { get; set; }

        public User? User { get; set; }

    }

    // interface segregation principle
    public class ExportableGoal : IExportGoalData
    {
        private readonly Goal _goal;
        public ExportableGoal(Goal goal)
        {
            _goal = goal;
        }
        string IExportGoalData.getGoalTitle() => _goal.Name;

        string IExportGoalData.getGoalStartDate() => _goal.startDate.ToString();

        string IExportGoalData.getGoalTargetDate() => _goal.targetDate.ToString();
    }


    public class GoalNotification : IGoalNotification
    {
        public string Name { get; set; }
        public GoalNotification(string goal) => Name = goal;
        public string GetDescription() => $"Goal: {Name}";
        public void SendReminder() => Console.WriteLine($"Reminder sent for goal: {Name}");
        public void StatusNotification() => Console.WriteLine($"Status notification for goal: {Name}");

    }




    public class ShortTermGoal : Goal
    {
        public int ReminderFrequencyDays { get; set; } = 7;
    }

    public class LongTermGoal : Goal
    {
        public List<string> Milestones { get; set; } = new();
    }

    public class SkillGoal : Goal
    {
        public string SkillCategory { get; set; }
        public int ProficiencyLevel { get; set; }
    }



    public class GoalHandlerResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class GoalRequest
    {
        public Goal Goal { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
    }

}
