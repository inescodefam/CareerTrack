namespace CareerTrack.Services.ExporterData
{
    public class ExcelExporter
    {
        public string FormatName => "Excel";
        public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public byte[] Export(ExportData data)
        {
            var csv = GenerateCsvContent(data);
            return System.Text.Encoding.UTF8.GetBytes(csv);
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
