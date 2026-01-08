
using CareerTrack.Models;
using CareerTrack.Repository;
using CareerTrack.Utilities;


/// 1.SINGLE RESPONSIBILITY PRINCIPLE(SRP) - bussines logic only
namespace CareerTrack.Services
{
    public class GoalService : IGoalService
    {
        private readonly IGoalRepository _repository;
        private readonly IDateTimeConverter _dateConverter;
        private readonly IProgressService _progressService;

        public GoalService(
            IGoalRepository repository,
            IDateTimeConverter dateConverter,
            IProgressService progressService)
        {
            _repository = repository;
            _dateConverter = dateConverter;
            _progressService = progressService;
        }

        public Goal CreateGoal(Goal goal, int userId)
        {
            goal.UserId = userId;
            _dateConverter.ConvertToUtc(goal);

            var createGoal = _repository.Create(goal);
            _progressService.InitializeProgress(createGoal.Id, userId);

            return createGoal;
        }

        public bool DeleteGoal(int goalId, int userId)
        {
            var goal = _repository.GetByIdAndUser(goalId, userId);
            if (goal == null) return false;

            return _repository.Delete(goalId);
        }

        public Goal? GetGoalById(int goalId, int userId)
        {
            return _repository.GetByIdAndUser(goalId, userId);
        }

        public IEnumerable<Goal> GetUserGoals(int userId)
        {
            return _repository.GetByUserId(userId);
        }

        public Goal UpdateGoal(Goal goal, int userId)
        {
            var existing = _repository.GetByIdAndUser(goal.Id, userId);
            if (existing == null) throw new UnauthorizedAccessException();

            _dateConverter.ConvertToUtc(goal);
            return _repository.Update(goal);
        }
    }
}
