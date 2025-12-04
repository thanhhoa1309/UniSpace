using System.ComponentModel.DataAnnotations;
using UniSpace.BusinessObject.Enums;

namespace UniSpace.BusinessObject.DTOs.BookingDTOs
{
    public class UpdateBookingDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Room is required")]
        public Guid RoomId { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Purpose is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Purpose must be between 10 and 500 characters")]
        public string Purpose { get; set; } = string.Empty;
    }
}
