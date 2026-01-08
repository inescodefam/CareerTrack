/// 1. SINGLE RESPONSIBILITY PRINCIPLE (SRP):data access

using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;


namespace CareerTrack.Repository
{
    public class GoalRepository : IGoalRepository
    {
        private readonly AppDbContext _context;
        public GoalRepository(AppDbContext context)
        {
            _context = context;
        }

        public Goal Create(Goal goal)
        {
            _context.Goals.Add(goal);
            _context.SaveChanges();
            return goal;
        }

        public bool Delete(int id)
        {
            var goal = _context.Goals.Find(id);
            if (goal == null) return false;

            _context.Goals.Remove(goal);
            _context.SaveChanges();
            return true;
        }

        public Goal? GetById(int id)
        {
            return _context.Goals
                 .Include(g => g.User)
                 .FirstOrDefault(g => g.Id == id);
        }

        public Goal? GetByIdAndUser(int goalId, int userId)
        {
            return _context.Goals
                .Include(g => g.User)
                .FirstOrDefault(g => g.Id == goalId && g.UserId == userId);
        }

        public IEnumerable<Goal> GetByUserId(int userId)
        {
            return _context.Goals
                 .Where(g => g.UserId == userId)
                 .Include(g => g.User)
                 .OrderByDescending(g => g.startDate)
                 .ToList();
        }

        public Goal Update(Goal goal)
        {
            _context.Goals.Update(goal);
            _context.SaveChangesAsync();
            return goal;
        }
    }
}
