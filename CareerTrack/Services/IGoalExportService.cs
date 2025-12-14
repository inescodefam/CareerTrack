namespace CareerTrack.Services
{
    public interface IGoalExportService
    {
        byte[] ExportGoal(int goalId, int userId, string format);
        IEnumerable<string> GetAvailableFormats();
    }
}
