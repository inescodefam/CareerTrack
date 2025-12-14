namespace CareerTrack.Models
{
    //3. Liscov Substitution Principle(LSP) applied

    public abstract class GoalProgressBase : IGoalProgress
    {
        public int Id { get; set; }
        public int GoalId { get; set; }
        public int UserId { get; set; }
        public GoalProgressData progressData { get; set; }
        public string? Notes { get; set; }

        // nikad ne smije baciti exception
        //public abstract string getProgress();

        public abstract string GetProgressDescription();

    }
    public class GoalProgress : GoalProgressBase
    {
        //public override string getProgress()
        //{
        //    if (progressData == null)
        //        return "No progress yet.";

        //    return $"Progress: {progressData.ProgressPercentage}%, " +
        //    $"Last Updated: {progressData.LastUpdated}" +
        //    $"Notes: {Notes}";

        //}

        public override string GetProgressDescription()
        {
            if (progressData == null)
                return "No progress yet.";

            return $"Progress: {progressData.ProgressPercentage}%";
        }
    }

    public class GoalTrackableProgress : GoalProgressBase
    {
        public int MilestonesCompleted { get; set; }
        public int TotalMilestones { get; set; }

        //public override string getProgress()
        //{
        //    if (TotalMilestones == 0)
        //        throw new InvalidOperationException("No milestones!");

        //    return MilestonesCompleted >= TotalMilestones ? "Completed!" : "Not yet.";
        //}

        public override string GetProgressDescription()
        {
            if (progressData == null)
                return $"Milestones Completed: {MilestonesCompleted}/{TotalMilestones}, no progress data yet.";

            return $"Milestones Completed: {MilestonesCompleted}/{TotalMilestones}, " +
                   $"Progress: {progressData.ProgressPercentage}%";
        }
    }

    public class GoalCompletionProgressStatus : GoalProgressBase
    {
        public bool IsCompleted { get; set; }
        public DateTime CompletionDate { get; set; }

        //public override string getProgress()
        //{
        //    if (!IsCompleted)
        //        throw new Exception("Goal not completed yet.");

        //    return JsonConvert.SerializeObject(this);
        //}

        public override string GetProgressDescription()
        {
            if (!IsCompleted)
            {
                if (progressData == null)
                    return "Goal not completed yet. No progress data.";

                return $"Goal not completed yet. Progress: {progressData.ProgressPercentage}%.";
            }

            var dateText = CompletionDate.ToShortDateString() ?? "unknown date";
            return $"Goal completed on {dateText}.";
        }
    }

    public class GoalProgressData
    {
        public int id { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public interface IGoalProgress
    {
        // Contract never throws exceptions, alwasy returns a valid string
        string GetProgressDescription();
    }
}


