namespace CareerTrack.Models
{
    public class GoalProgress
    {
        public int Id { get; set; }
        public int GoalId { get; set; }
        public int UserId { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? Notes { get; set; }
    }
}
