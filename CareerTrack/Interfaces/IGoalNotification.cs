namespace CareerTrack.Interfaces
{
    public interface IGoalNotification 
    {
        string GetDescription();
        void SendReminder();
        void StatusNotification();
    }
}
