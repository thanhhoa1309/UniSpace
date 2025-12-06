using System.ComponentModel.DataAnnotations;
using UniSpace.BusinessObject.Enums;

namespace UniSpace.BusinessObject.DTOs.ScheduleDTOs
{
    public class BulkCreateScheduleDto
    {
        [Required(ErrorMessage = "At least one room must be selected")]
        [MinLength(1, ErrorMessage = "Please select at least one room")]
        public List<Guid> RoomIds { get; set; } = new();

        [Required(ErrorMessage = "Schedule type is required")]
        public ScheduleType ScheduleType { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "At least one day must be selected")]
        [MinLength(1, ErrorMessage = "Please select at least one day of week")]
        public List<int> DaysOfWeek { get; set; } = new();

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
        /// If true, skip rooms with conflicts instead of failing entirely
        /// </summary>
        public bool SkipConflicts { get; set; } = false;
    }

    public class BulkCreateScheduleResultDto
    {
        public int TotalRoomsProcessed { get; set; }
        public int SuccessfulSchedules { get; set; }
        public int FailedSchedules { get; set; }
        public List<ScheduleDto> CreatedSchedules { get; set; } = new();
        public List<BulkCreateErrorDto> Errors { get; set; } = new();
    }

    public class BulkCreateErrorDto
    {
        public Guid RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int? DayOfWeek { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
