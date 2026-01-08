using CareerTrack.Models;

namespace CareerTrack.Handlers
{
    public class GoalBusinessRuleHandler : GoalHandler
    {
        private readonly AppDbContext _context;

        public GoalBusinessRuleHandler(AppDbContext context)
        {
            _context = context;
        }

        public override GoalHandlerResult Handle(GoalRequest request)
        {
            var result = new GoalHandlerResult();

            var activeGoalsCount = _context.Goals
                .Where(g => g.UserId == request.UserId && g.endDate == null)
                .Count();

            if (request.Action == "Create" && activeGoalsCount >= 10)
            {
                result.Success = false;
                result.Message = "You cannot have more than 10 active goals";
                return result;
            }

            return base.Handle(request);
        }
    }
}
