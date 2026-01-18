using CareerTrack.Models;

namespace CareerTrack.Services
{

    // 4. INTERFACE SEGREGATION PRINCIPLE (ISP) 
    public class ProgressService : IProgressService
    {
        private readonly AppDbContext _context;

        public ProgressService(AppDbContext context)
        {
            _context = context;
        }

        public void InitializeProgress(int goalId, int userId)
        {
            var progress = new GoalProgress
            {
                GoalId = goalId,
                UserId = userId,
                progressData = new GoalProgressData
                {
                    ProgressPercentage = 0,
                    LastUpdated = DateTime.UtcNow
                },
                Notes = "Goal created"
            };

            _context.GoalProgress.Add(progress);
            _context.SaveChanges();
        }

        public GoalProgress? GetProgress(int goalId, int userId)
        {
            return _context.GoalProgress
                .FirstOrDefault(gp => gp.GoalId == goalId && gp.UserId == userId);
        }

        public void UpdateProgress(int goalId, int userId, int percentage, string? notes = null)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentException("Progress must be between 0 and 100");

            var progress = GetProgress(goalId, userId);
            if (progress == null)
                throw new InvalidOperationException("Progress record not found");

            var oldPercentage = progress.progressData.ProgressPercentage;

            progress.progressData.ProgressPercentage = percentage;
            progress.progressData.LastUpdated = DateTime.UtcNow;
            progress.Notes = notes;

            var historyEntry = new GoalProgress
            {
                GoalId = goalId,
                UserId = userId,
                progressData = new GoalProgressData
                {
                    ProgressPercentage = percentage,
                    LastUpdated = DateTime.UtcNow
                },
                Notes = notes ?? $"Progress updated from {oldPercentage}% to {percentage}%"
            };

            _context.GoalProgress.Add(historyEntry);

            if (percentage >= 100)
            {
                var goal = _context.Goals.Find(goalId);
                if (goal != null && !goal.endDate.HasValue)
                {
                    goal.endDate = DateTime.UtcNow;
                }
            }

            _context.SaveChanges();
        }

        public IEnumerable<GoalProgress> GetProgressHistory(int goalId, int userId)
        {
            return _context.GoalProgress
                .Where(pu => pu.GoalId == goalId && pu.UserId == userId)
                .OrderByDescending(pu => pu.progressData.LastUpdated)
                .ToList();
        }
    }
}
