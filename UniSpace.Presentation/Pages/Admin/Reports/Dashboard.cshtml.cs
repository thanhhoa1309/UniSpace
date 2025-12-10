using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Reports
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomReportService _roomReportService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IBookingService bookingService,
            IRoomReportService roomReportService,
            ILogger<DashboardModel> logger)
        {
            _bookingService = bookingService;
            _roomReportService = roomReportService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string Period { get; set; } = "week"; // day, week, month

        // Statistics
        public int TotalBookings { get; set; }
        public int TotalReports { get; set; }
        public int PendingBookings { get; set; }
        public int OpenReports { get; set; }

        // Chart Data
        public Dictionary<string, int> BookingsByRoomType { get; set; } = new();
        public Dictionary<string, int> ReportsByStatus { get; set; } = new();
        public Dictionary<string, int> BookingsTrend { get; set; } = new();
        public Dictionary<string, int> ReportsTrend { get; set; } = new();
        public Dictionary<string, int> BookingsByStatus { get; set; } = new();
        public Dictionary<string, int> ReportsByIssueType { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var (fromDate, toDate) = GetDateRange(Period);

                // Get all bookings and reports for the period
                var bookingsResult = await _bookingService.GetBookingsAsync(
                    pageNumber: 1,
                    pageSize: 10000,
                    fromDate: fromDate,
                    toDate: toDate
                );

                var reportsResult = await _roomReportService.GetRoomReportsAsync(
                    pageNumber: 1,
                    pageSize: 10000
                );

                var bookings = bookingsResult.ToList();
                // reportsResult is already a Pagination which inherits from List
                var reports = reportsResult
                    .Where(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
                    .ToList();

                // Calculate statistics
                TotalBookings = bookings.Count;
                TotalReports = reports.Count;
                PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending);
                OpenReports = reports.Count(r => r.Status == ReportStatus.Open);

                // Bookings by Room Type
                BookingsByRoomType = bookings
                    .GroupBy(b => b.RoomTypeDisplay)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Bookings by Status
                BookingsByStatus = bookings
                    .GroupBy(b => b.StatusDisplay)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Reports by Status
                ReportsByStatus = reports
                    .GroupBy(r => r.StatusDisplay)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Reports by Issue Type
                ReportsByIssueType = reports
                    .GroupBy(r => r.IssueType)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Trend data
                BookingsTrend = GetBookingsTrend(bookings, fromDate, toDate);
                ReportsTrend = GetReportsTrend(reports, fromDate, toDate);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return Page();
            }
        }

        private (DateTime fromDate, DateTime toDate) GetDateRange(string period)
        {
            var now = DateTime.UtcNow;
            var toDate = now;
            DateTime fromDate;

            switch (period.ToLower())
            {
                case "day":
                    fromDate = now.Date;
                    toDate = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "week":
                    fromDate = now.Date.AddDays(-(int)now.DayOfWeek);
                    toDate = fromDate.AddDays(7).AddTicks(-1);
                    break;
                case "month":
                    fromDate = new DateTime(now.Year, now.Month, 1);
                    toDate = fromDate.AddMonths(1).AddTicks(-1);
                    break;
                default:
                    fromDate = now.Date.AddDays(-7);
                    toDate = now;
                    break;
            }

            return (fromDate, toDate);
        }

        private Dictionary<string, int> GetBookingsTrend(
            List<UniSpace.BusinessObject.DTOs.BookingDTOs.BookingDto> bookings,
            DateTime fromDate,
            DateTime toDate)
        {
            var trend = new Dictionary<string, int>();

            if (Period == "day")
            {
                // Hourly breakdown
                for (int hour = 0; hour < 24; hour++)
                {
                    var label = $"{hour:D2}:00";
                    var count = bookings.Count(b => b.CreatedAt.Hour == hour);
                    trend[label] = count;
                }
            }
            else if (Period == "week")
            {
                // Daily breakdown
                for (int day = 0; day < 7; day++)
                {
                    var date = fromDate.AddDays(day);
                    var label = date.ToString("ddd");
                    var count = bookings.Count(b => b.CreatedAt.Date == date.Date);
                    trend[label] = count;
                }
            }
            else // month
            {
                // Weekly breakdown
                var weeks = (int)Math.Ceiling((toDate - fromDate).TotalDays / 7.0);
                for (int week = 0; week < weeks; week++)
                {
                    var weekStart = fromDate.AddDays(week * 7);
                    var weekEnd = weekStart.AddDays(7);
                    var label = $"Week {week + 1}";
                    var count = bookings.Count(b => b.CreatedAt >= weekStart && b.CreatedAt < weekEnd);
                    trend[label] = count;
                }
            }

            return trend;
        }

        private Dictionary<string, int> GetReportsTrend(
            List<UniSpace.BusinessObject.DTOs.RoomReportDTOs.RoomReportDto> reports,
            DateTime fromDate,
            DateTime toDate)
        {
            var trend = new Dictionary<string, int>();

            if (Period == "day")
            {
                // Hourly breakdown
                for (int hour = 0; hour < 24; hour++)
                {
                    var label = $"{hour:D2}:00";
                    var count = reports.Count(r => r.CreatedAt.Hour == hour);
                    trend[label] = count;
                }
            }
            else if (Period == "week")
            {
                // Daily breakdown
                for (int day = 0; day < 7; day++)
                {
                    var date = fromDate.AddDays(day);
                    var label = date.ToString("ddd");
                    var count = reports.Count(r => r.CreatedAt.Date == date.Date);
                    trend[label] = count;
                }
            }
            else // month
            {
                // Weekly breakdown
                var weeks = (int)Math.Ceiling((toDate - fromDate).TotalDays / 7.0);
                for (int week = 0; week < weeks; week++)
                {
                    var weekStart = fromDate.AddDays(week * 7);
                    var weekEnd = weekStart.AddDays(7);
                    var label = $"Week {week + 1}";
                    var count = reports.Count(r => r.CreatedAt >= weekStart && r.CreatedAt < weekEnd);
                    trend[label] = count;
                }
            }

            return trend;
        }
    }
}
