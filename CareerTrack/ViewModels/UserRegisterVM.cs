using System.ComponentModel.DataAnnotations;

namespace CareerTrack.ViewModels
{
    public class UserRegisterVM
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Username must be at between 2 and 100 characters")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "You must provide a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be at least 2 characters long")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;


        [Required(ErrorMessage = "last name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be at least 2 characters long")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;


        [Phone(ErrorMessage = "You must provide a valid phone number")]
        [Display(Name = "Phone number")]
        public string Phone { get; set; } = string.Empty;
    }
}
