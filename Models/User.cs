using System.ComponentModel.DataAnnotations;

namespace dprmitory.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Username must contain only letters")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be at least 4 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain only letters and spaces")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public UserRole Role { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public enum UserRole
    {
        Student,
        Proctor,
        Director
    }
}
