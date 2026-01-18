using CareerTrack.Models;

namespace CareerTrack.Services
{
    public class GoalExportService : IGoalExportService
    {
        private readonly IEnumerable<IExporter> _exporters;
        private readonly IGoalService _goalService;
        private readonly AppDbContext _context;

        public GoalExportService(
            IEnumerable<IExporter> exporters,
            IGoalService goalService,
            IProgressService progressService,
            AppDbContext context)
        {
            _exporters = exporters;
            _goalService = goalService;
            _context = context;
        }

        // open for extension, closed for modification principle

        public byte[] ExportGoal(int goalId, int userId, string format)
        {
            var exporter = _exporters.FirstOrDefault(e =>
                e.FormatName.Equals(format, StringComparison.OrdinalIgnoreCase));

            if (exporter == null)
                throw new ArgumentException($"Format '{format}' not supported");
            var user = _context.Users.Find(userId);
            if (user == null) throw new UnauthorizedAccessException();

            var goal = _goalService.GetGoalById(goalId, userId);
            if (goal == null) throw new UnauthorizedAccessException();

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
