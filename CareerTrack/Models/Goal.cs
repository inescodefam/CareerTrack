using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models
{
    public class Goal
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [Required]
        public DateTime targetDate { get; set; }
        [Required]
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

    }
}
