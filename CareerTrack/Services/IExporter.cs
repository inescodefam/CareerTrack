namespace CareerTrack.Services
{
    public interface IExporter
    {
        // IExporter interface allows adding new export formats and there is no need to modify GoalExportService
        string FormatName { get; }
        string ContentType { get; }
        byte[] Export(ExportData data);

    }
}
