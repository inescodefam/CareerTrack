using CareerTrack.Models;
/// open/close principle
namespace CareerTrack.Services
{
    public class ExportData
    {
        public Goal Goal { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}