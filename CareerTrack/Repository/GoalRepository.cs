/// 1. SINGLE RESPONSIBILITY PRINCIPLE (SRP):data access

using CareerTrack.Models;


namespace CareerTrack.Repository
{
    public class GoalRepository : IGoalRepository
    {
        public Goal Create(Goal goal)
        {
            throw new NotImplementedException();
        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Goal? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Goal? GetByIdAndUser(int goalId, int userId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Goal> GetByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public Goal Update(Goal goal)
        {
            throw new NotImplementedException();
        }
    }
}
