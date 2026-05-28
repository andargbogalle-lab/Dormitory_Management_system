using System.ComponentModel.DataAnnotations;

namespace dprmitory.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Room number is required")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Room number must contain only letters and numbers")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Room number must be between 1 and 10 characters")]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
        public int Capacity { get; set; }

        [Range(0, 20, ErrorMessage = "Current occupancy must be between 0 and 20")]
        [Display(Name = "Current Occupancy")]
        public int CurrentOccupancy { get; set; }

        [Required(ErrorMessage = "Room type is required")]
        public RoomType Type { get; set; }

        public RoomStatus Status { get; set; } = RoomStatus.Available;
        
        public List<Student> Students { get; set; } = new();
    }

    public enum RoomType
    {
        Single,
        Double,
        Triple,
        Quad,
        Six
    }

    public enum RoomStatus
    {
        Available,
        Occupied,
        Full,
        Maintenance
    }
}
