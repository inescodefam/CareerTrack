using CareerTrack.Models;

namespace CareerTrack.Services
{

    // SRP violation
    // open for extension, closed for modification principle is violated

    //public class GoalExportService
    //{
    //    public byte[] ExportGoal(Goal goal, string format)
    //    {
    //        // Violation: Must modify this method to add new formats
    //        if (format == "PDF")
    //        {
    //            return GeneratePdf(goal);
    //        }
    //        else if (format == "Excel")
    //        {
    //            return GenerateExcel(goal);
    //        }
    //        else if (format == "Word") // New format requires modification!
    //        {
    //            return GenerateWord(goal);
    //        }
    //        throw new ArgumentException("Unsupported format");
    //    }
    //}
    public class GoalExportService : IGoalExportService
    {
        private readonly IEnumerable<IExporter> _exporters;
        private readonly IGoalService _goalService;
        private readonly IProgressService _progressService;
        private readonly AppDbContext _context;

        public GoalExportService(
            IEnumerable<IExporter> exporters,
            IGoalService goalService,
            IProgressService progressService,
            AppDbContext context)
        {
            _exporters = exporters;
            _goalService = goalService;
            _progressService = progressService;
            _context = context;
        }

        // open for extension, closed for modification principle

        public byte[] ExportGoal(int goalId, int userId, string format)
        {
            var exporter = _exporters.FirstOrDefault(e =>
                e.FormatName.Equals(format, StringComparison.OrdinalIgnoreCase));

            if (exporter == null)
                throw new ArgumentException($"Format '{format}' not supported");

            var goal = _goalService.GetGoalById(goalId, userId);
            if (goal == null) throw new UnauthorizedAccessException();

            var user = _context.Users.Find(userId);

            // interface segretation

            ExportableUser userData = new(user);
            ExportableGoal data = new(goal);

            return exporter.Export(userData, data);
        }

        public IEnumerable<string> GetAvailableFormats()
        {
            return _exporters.Select(e => e.FormatName);
        }
    }
}
