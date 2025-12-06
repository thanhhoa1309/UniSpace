using System.ComponentModel.DataAnnotations;
using UniSpace.BusinessObject.Enums;

namespace UniSpace.BusinessObject.DTOs.ScheduleDTOs
{
    public class CreateScheduleDto
    {
        [Required(ErrorMessage = "Room is required")]
        public Guid RoomId { get; set; }

        [Required(ErrorMessage = "Schedule type is required")]
        public ScheduleType ScheduleType { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Day of week is required")]
        [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Break time in minutes between schedules (default: 15 minutes)
        /// </summary>
        [Range(0, 120, ErrorMessage = "Break time must be between 0 and 120 minutes")]
        public int BreakTimeMinutes { get; set; } = 15;

        /// <summary>
        /// If true, this is a one-time schedule (not recurring).
        /// StartDate and EndDate should be the same.
        /// </summary>
        public bool IsOneTime { get; set; } = false;

        /// <summary>
        /// Custom validation: Validates that dates are consistent with schedule type
        /// </summary>
        public bool IsValid(out string? errorMessage)
        {
            errorMessage = null;

            // Check time validity
            if (StartTime >= EndTime)
            {
                errorMessage = "Start time must be before end time";
                return false;
            }

            // For one-time schedules, dates should be the same
            if (IsOneTime)
            {
                if (StartDate.Date != EndDate.Date)
                {
                    errorMessage = "For one-time schedules, Start date and End date must be the same";
                    return false;
                }

                // Day of week should match the date
                if ((int)StartDate.DayOfWeek != DayOfWeek)
                {
                    errorMessage = $"Day of week ({DayOfWeek}) does not match the selected date ({StartDate:yyyy-MM-dd})";
                    return false;
                }
            }
            // For recurring schedules
            else
            {
                if (StartDate.Date > EndDate.Date)
                {
                    errorMessage = "Start date must be before or equal to end date";
                    return false;
                }
            }

            return true;
        }
    }
}
