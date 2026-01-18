namespace CareerTrack.Services.ExporterData
{

    // open close principle
    public class PdfExporter : IExporter
    {
        public string FormatName => "PDF";
        public string ContentType => "application/pdf";

        public byte[] Export(IExportUserData userData, IExportGoalData goalData)
        {
            var html = GenerateHtmlReport(userData, goalData);
            return System.Text.Encoding.UTF8.GetBytes($"PDF: {html}");
        }

        private static string GenerateHtmlReport(IExportUserData data, IExportGoalData goalData)
        {
            return $@"
                <html>
                <head><title>Goal Report: {goalData.getGoalTitle()}</title></head>
                <body>
                    <h1>{goalData.getGoalTitle()}</h1>
                    <p>Owner: {data.getUserName()} </p>
                    <p>Start: {goalData.getGoalStartDate()}</p>
                    <p>Target: {goalData.getGoalTargetDate()}</p>
                </body>
                </html>";
        }
    }
}
