using CareerTrack.Services.ExporterData;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CareerTrack.Models
{
    public class User
    {

        [JsonRequired]
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string PasswordSalt { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [JsonRequired]
        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        public bool IsAdmin { get; set; } = false;
    }

    // interface segregation principle
    public class ExportableUser : IExportUserData
    {
        private readonly User _user;
        public ExportableUser(User user)
        {
            _user = user;
        }

        string IExportUserData.getUserName() => $"{_user.FirstName} {_user.LastName}";
    }
}


