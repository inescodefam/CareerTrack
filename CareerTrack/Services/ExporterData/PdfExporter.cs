namespace CareerTrack.Services.ExporterData
{

    // open close principle
    public class PdfExporter : IExporter
    {
        public string FormatName => "PDF";
        public string ContentType => "application/pdf";

        /* // 4. interface segregation principle
         
        public byte[] Export(ExportData data)
        {
            var html = GenerateHtmlReport(data);
            return System.Text.Encoding.UTF8.GetBytes($"PDF: {html}");
        }

        private string GenerateHtmlReport(ExportData data)
        {
            return $@"
                <html>
                <head><title>Goal Report: {data.Goal.Name}</title></head>
                <body>
                    <h1>{data.Goal.Name}</h1>
                    <p>Owner: {data.User.FirstName} {data.User.LastName}</p>
                    <p>Start: {data.Goal.startDate:yyyy-MM-dd}</p>
                    <p>Target: {data.Goal.targetDate:yyyy-MM-dd}</p>
                </body>
                </html>";
        }
         */

        public byte[] Export(IExportUserData userData, IExportGoalData goalData)
        {
            var html = GenerateHtmlReport(userData, goalData);
            return System.Text.Encoding.UTF8.GetBytes($"PDF: {html}");
        }

        private string GenerateHtmlReport(IExportUserData data, IExportGoalData goalData)
        {
            return $@"
                <html>
                <head><title>Goal Report: {goalData.getGoalTitle}</title></head>
                <body>
                    <h1>{goalData.getGoalTitle}</h1>
                    <p>Owner: {data.getUserName} </p>
                    <p>Start: {goalData.getGoalStartDate:yyyy-MM-dd}</p>
                    <p>Target: {goalData.getGoalTargetDate:yyyy-MM-dd}</p>
                </body>
                </html>";
        }
    }
}
