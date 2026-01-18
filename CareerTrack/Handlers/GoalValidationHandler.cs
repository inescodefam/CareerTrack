using CareerTrack.Models;

namespace CareerTrack.Handlers
{
    public class GoalValidationHandler : GoalHandler
    {
        public override GoalHandlerResult Handle(GoalRequest request)
        {
            var result = new GoalHandlerResult();
            if (request.Goal != null)
            {

                if (string.IsNullOrWhiteSpace(request.Goal.Name))
                    result.Errors.Add("Goal name is required");

                if (request.Goal.Name?.Length > 150)
                    result.Errors.Add("Goal name must be 150 characters or less");

                if (request.Goal.targetDate <= DateTime.UtcNow)
                    result.Errors.Add("Target date must be in the future");

                if (request.Goal.startDate >= request.Goal.targetDate)
                    result.Errors.Add("Start date must be before target date");

                if (result.Errors.Count != 0)
                {
                    result.Success = false;
                    result.Message = "Validation failed";
                    return result; //stop
                }

            }
            return base.Handle(request);
        }
    }
}
