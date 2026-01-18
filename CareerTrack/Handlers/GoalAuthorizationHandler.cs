using CareerTrack.Models;

namespace CareerTrack.Handlers
{
    public class GoalAuthorizationHandler : GoalHandler
    {
        private readonly AppDbContext _context;

        public GoalAuthorizationHandler(AppDbContext context)
        {
            _context = context;
        }

        public override GoalHandlerResult Handle(GoalRequest request)
        {
            var result = new GoalHandlerResult();

            if (request.Action == "Delete" || request.Action == "Update")
            {
                var existingGoal = _context.Goals.Find(request.Goal.Id);

                if (existingGoal?.UserId != request.UserId)
                {
                    result.Success = false;
                    result.Message = "You are not authorized to modify this goal";
                    return result; //stop
                }
            }

            // continue to next handler
            return base.Handle(request);
        }
    }
}
