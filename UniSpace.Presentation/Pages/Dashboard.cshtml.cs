using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.DTOs.RoomReportDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Domain.Interfaces;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly IBookingService _bookingService;
        private readonly IRoomReportService _roomReportService;
        private readonly IClaimsService _claimsService;

        public DashboardModel(
            ILogger<DashboardModel> logger,
            IBookingService bookingService,
            IRoomReportService roomReportService,
            IClaimsService claimsService)
        {
            _logger = logger;
            _bookingService = bookingService;
            _roomReportService = roomReportService;
            _claimsService = claimsService;
        }

        public string? UserEmail { get; set; }
        public string? UserRole { get; set; }
        public Guid UserId { get; set; }

        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ApprovedBookings { get; set; }
        public int TotalReports { get; set; }

        public List<BookingDto> UpcomingBookings { get; set; } = new List<BookingDto>();
        public List<BookingDto> RecentBookings { get; set; } = new List<BookingDto>();

        public async Task OnGetAsync()
        {
            try
            {
                UserEmail = User.Identity?.Name;
                UserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                UserId = _claimsService.GetCurrentUserId;

                // Get booking statistics
                TotalBookings = await _bookingService.GetUserBookingsCountAsync(UserId);
                
                var bookings = await _bookingService.GetUserBookingsAsync(UserId);
                
                PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending);
                ApprovedBookings = bookings.Count(b => b.Status == BookingStatus.Approved);

                // Get upcoming approved bookings (next 5)
                UpcomingBookings = bookings
                    .Where(b => b.Status == BookingStatus.Approved && b.StartTime > DateTime.UtcNow)
                    .OrderBy(b => b.StartTime)
                    .Take(5)
                    .ToList();

                // Get recent bookings (last 5)
                RecentBookings = bookings
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToList();

                // Get report count
                TotalReports = await _roomReportService.GetUserReportsCountAsync(UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
            }
        }
    }
}
