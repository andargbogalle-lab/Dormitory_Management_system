using System.ComponentModel.DataAnnotations;

namespace dprmitory.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student ID is required")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Student ID must contain only letters and numbers")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Student ID must be between 3 and 20 characters")]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be at least 4 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain only letters and spaces")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number must contain only numbers")]
        [StringLength(15, MinimumLength = 9, ErrorMessage = "Phone number must be between 9 and 15 digits")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Department must contain only letters and spaces")]
        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string Department { get; set; } = string.Empty;

        public int? RoomId { get; set; }
        public Room? Room { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Check-In Date")]
        public DateTime? CheckInDate { get; set; }

        [Display(Name = "Check-Out Date")]
        public DateTime? CheckOutDate { get; set; }

        [Display(Name = "Checked In")]
        public bool IsCheckedIn { get; set; }

        public AvailabilityStatus Status { get; set; } = AvailabilityStatus.Absent;

        /// <summary>Timestamp when the student was last marked Present. Used to auto-reset after 24 hours.</summary>
        [Display(Name = "Marked Present At")]
        public DateTime? MarkedPresentAt { get; set; }
    }

    public enum AvailabilityStatus
    {
        Present = 0,
        Absent = 1
    }
}
