using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.ScheduleDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Schedule
{
    [Authorize(Roles = "Admin")]
    public class CalendarModel : PageModel
    {
        private readonly IScheduleService _scheduleService;
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly ILogger<CalendarModel> _logger;

        public CalendarModel(
            IScheduleService scheduleService,
            IRoomService roomService,
            ICampusService campusService,
            ILogger<CalendarModel> logger)
        {
            _scheduleService = scheduleService;
            _roomService = roomService;
            _campusService = campusService;
            _logger = logger;
        }

        public List<ScheduleDto> Schedules { get; set; } = new();
        public List<WeekViewDto> WeekView { get; set; } = new();
        public DateTime CurrentWeekStart { get; set; }
        public DateTime CurrentWeekEnd { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? ViewDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? FilterRoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? FilterCampusId { get; set; }

        [BindProperty(SupportsGet = true)]
        public ScheduleType? FilterType { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Set current week
                var viewDate = ViewDate ?? DateTime.Now;
                CurrentWeekStart = viewDate.Date.AddDays(-(int)viewDate.DayOfWeek);
                CurrentWeekEnd = CurrentWeekStart.AddDays(7);

                // Get schedules for date range
                var schedules = await _scheduleService.GetSchedulesByDateRangeAsync(
                    CurrentWeekStart,
                    CurrentWeekEnd
                );

                // Apply filters
                if (FilterRoomId.HasValue)
                {
                    schedules = schedules.Where(s => s.RoomId == FilterRoomId.Value).ToList();
                }

                if (FilterCampusId.HasValue)
                {
                    var campus = await _campusService.GetCampusByIdAsync(FilterCampusId.Value);
                    if (campus != null)
                    {
                        schedules = schedules.Where(s => s.CampusName == campus.Name).ToList();
                    }
                }

                if (FilterType.HasValue)
                {
                    schedules = schedules.Where(s => s.ScheduleType == FilterType.Value).ToList();
                }

                Schedules = schedules;

                // Build week view
                BuildWeekView();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading calendar view");
            }
        }

        private void BuildWeekView()
        {
            WeekView = new List<WeekViewDto>();

            // Group schedules by time slots
            var timeSlots = Schedules
                .Select(s => s.StartTime)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            foreach (var timeSlot in timeSlots)
            {
                var weekRow = new WeekViewDto
                {
                    TimeSlot = timeSlot
                };

                // For each day of week
                for (int day = 0; day < 7; day++)
                {
                    var daySchedules = Schedules
                        .Where(s => s.DayOfWeek == day && s.StartTime == timeSlot)
                        .ToList();

                    weekRow.DaySchedules[day] = daySchedules;
                }

                WeekView.Add(weekRow);
            }
        }

        public class WeekViewDto
        {
            public TimeSpan TimeSlot { get; set; }
            public Dictionary<int, List<ScheduleDto>> DaySchedules { get; set; } = new();

            public WeekViewDto()
            {
                for (int i = 0; i < 7; i++)
                {
                    DaySchedules[i] = new List<ScheduleDto>();
                }
            }
        }
    }
}
