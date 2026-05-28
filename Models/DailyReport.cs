using System.ComponentModel.DataAnnotations;

namespace dprmitory.Models
{
    public class DailyReport
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Report date is required")]
        [Display(Name = "Report Date")]
        public DateTime ReportDate { get; set; } = DateTime.Now.Date;

        [Required(ErrorMessage = "Prepared by is required")]
        [Display(Name = "Prepared By")]
        public string PreparedBy { get; set; } = string.Empty;

        public int PreparedByUserId { get; set; }

        [Required(ErrorMessage = "Total students is required")]
        [Display(Name = "Total Students")]
        public int TotalStudents { get; set; }

        [Required(ErrorMessage = "Present count is required")]
        [Display(Name = "Students Present")]
        public int StudentsPresent { get; set; }

        [Required(ErrorMessage = "Absent count is required")]
        [Display(Name = "Students Absent")]
        public int StudentsAbsent { get; set; }

        [Required(ErrorMessage = "Total rooms is required")]
        [Display(Name = "Total Rooms")]
        public int TotalRooms { get; set; }

        [Required(ErrorMessage = "Occupied rooms is required")]
        [Display(Name = "Occupied Rooms")]
        public int OccupiedRooms { get; set; }

        [Required(ErrorMessage = "Available rooms is required")]
        [Display(Name = "Available Rooms")]
        public int AvailableRooms { get; set; }

        [Display(Name = "Total Capacity")]
        public int TotalCapacity { get; set; }

        [Display(Name = "Current Occupancy")]
        public int CurrentOccupancy { get; set; }

        [Display(Name = "Check-ins Today")]
        public int CheckInsToday { get; set; }

        [Display(Name = "Check-outs Today")]
        public int CheckOutsToday { get; set; }

        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters")]
        [Display(Name = "Remarks/Notes")]
        public string? Remarks { get; set; }

        [StringLength(1000, ErrorMessage = "Issues cannot exceed 1000 characters")]
        [Display(Name = "Issues Reported")]
        public string? Issues { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ReportStatus Status { get; set; } = ReportStatus.Draft;
    }

    public enum ReportStatus
    {
        Draft,
        Submitted,
        Reviewed
    }
}
