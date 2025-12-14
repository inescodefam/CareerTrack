using CareerTrack.Models;

namespace CareerTrack.Utilities
{
    public interface IDateTimeConverter
    {
        void ConvertToUtc(Goal goal);
    }
}
