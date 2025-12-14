using CareerTrack.Models;

namespace CareerTrack.Services
{
    /*
     *  4. INTERFACE SEGREGATION PRINCIPLE (ISP)
     *  
         public interface IProgressService
    {
        // debeli interface, forsiranje nepotrebnih implemetacija

        GoalProgress GetProgress(int goalId, int userId);
        UpdateProgress(int goalId, int percentage, string notes);
        List<ProgressUpdate> GetHistory(int goalId, int userId);
        byte[] GenerateProgressReportPdf(int goalId);
        byte[] GenerateProgressReportExcel(int goalId); 
        SendProgressNotification(int goalId); 
        ArchiveProgress(int goalId); 
    }
     */
    public interface IProgressService
    {
        void InitializeProgress(int goalId, int userId);
        GoalProgress? GetProgress(int goalId, int userId);
        void UpdateProgress(int goalId, int userId, int percentage, string? notes = null);
        IEnumerable<ProgressUpdate> GetProgressHistory(int goalId, int userId);

    }

    public interface IProgressReportService
    {
        byte[] GenerateProgressReport(int goalId, int userId, string format);
    }

    public interface IProgressNotificationService
    {
        bool NotifyProgressMilestone(int goalId, int percentage);

        bool NotifyGoalDeadlineAsync(int goalId);
    }
}