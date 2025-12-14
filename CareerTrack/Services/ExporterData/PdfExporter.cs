namespace CareerTrack.Services.ExporterData
{

    // open close principle
    public class PdfExporter : IExporter
    {
        public string FormatName => "PDF";
        public string ContentType => "application/pdf";

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
    }
}
