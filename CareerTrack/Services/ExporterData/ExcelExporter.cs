namespace CareerTrack.Services.ExporterData
{
    // open / close principle

    public class ExcelExporter : IExporter
    {
        public string FormatName => "Excel";
        public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public byte[] Export(ExportData data) // kako bi bilo da nije ISP
        {
            var csv = GenerateCsvContent(data);
            return System.Text.Encoding.UTF8.GetBytes(csv);
        }

        public byte[] Export(IExportUserData userData, IExportGoalData goalData)
        {
            throw new NotImplementedException();
        }

        private string GenerateCsvContent(ExportData data)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Goal Name,Owner,Start Date,Target Date,Progress");
            sb.AppendLine($"{data.Goal.Name},{data.User.FirstName} {data.User.LastName}," +
                         $"{data.Goal.startDate:yyyy-MM-dd},{data.Goal.targetDate:yyyy-MM-dd},");
            return sb.ToString();
        }
    }
}
