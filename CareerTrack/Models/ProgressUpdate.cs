namespace CareerTrack.Models
{
    public class ProgressUpdate
    {

        public int Id { get; set; }
        public int GoalId { get; set; }
        public int UserId { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Notes { get; set; }
    }
}
