namespace CareerTrack.Services.ExporterData
{
    public interface IExportUserData
    {
        public string getUserName();
    }


    public interface IExportGoalData
    {
        public string getGoalTitle();
        public string getGoalStartDate();
        public string getGoalTargetDate();
    }
}
