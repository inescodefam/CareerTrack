using CareerTrack.Services.ExporterData;

namespace CareerTrack.Services
{
    public interface IExporter
    {
        // Interface segregation principle
        // Export ne ovisi o User i Goal modelima i njihovim metodama direktno
        string FormatName { get; }
        string ContentType { get; }

        byte[] Export(IExportUserData userData, IExportGoalData goalData);

    }
}
