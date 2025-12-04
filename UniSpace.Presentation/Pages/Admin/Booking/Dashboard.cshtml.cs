using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Booking
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IBookingService bookingService,
            ILogger<DashboardModel> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        // Statistics
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ApprovedBookings { get; set; }
        public int RejectedBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }

        // Lists
        public List<BookingDto> RecentBookings { get; set; } = new();
        public List<BookingDto> PendingBookingsList { get; set; } = new();
        public List<BookingDto> TodayBookings { get; set; } = new();

        // Chart Data
        public Dictionary<string, int> BookingStatusDistribution { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Get all bookings with large page size for statistics
                var allBookingsPagination = await _bookingService.GetBookingsAsync(pageNumber: 1, pageSize: 10000);
                var allBookings = allBookingsPagination.ToList();

                // Calculate statistics
                TotalBookings = allBookingsPagination.TotalCount;
                PendingBookings = allBookings.Count(b => b.Status == BookingStatus.Pending);
                ApprovedBookings = allBookings.Count(b => b.Status == BookingStatus.Approved);
                RejectedBookings = allBookings.Count(b => b.Status == BookingStatus.Rejected);
                CompletedBookings = allBookings.Count(b => b.Status == BookingStatus.Completed);
                CancelledBookings = allBookings.Count(b => b.Status == BookingStatus.Cancelled);

                // Recent bookings (top 10)
                RecentBookings = allBookings
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(10)
                    .ToList();

                // Pending bookings (top 10)
                PendingBookingsList = allBookings
                    .Where(b => b.Status == BookingStatus.Pending)
                    .OrderBy(b => b.StartTime)
                    .Take(10)
                    .ToList();

                // Today's bookings
                var today = DateTime.UtcNow.Date;
                TodayBookings = allBookings
                    .Where(b => b.StartTime.Date == today)
                    .OrderBy(b => b.StartTime)
                    .ToList();

                // Chart data - Booking Status Distribution
                BookingStatusDistribution = new Dictionary<string, int>
                {
                    { "Pending", PendingBookings },
                    { "Approved", ApprovedBookings },
                    { "Rejected", RejectedBookings },
                    { "Completed", CompletedBookings },
                    { "Cancelled", CancelledBookings }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking dashboard data");

                // Initialize with default values
                TotalBookings = 0;
                PendingBookings = 0;
                ApprovedBookings = 0;
                RejectedBookings = 0;
                CompletedBookings = 0;
                CancelledBookings = 0;
            }
        }

        public string GetStatusBadgeClass(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "bg-warning text-dark",
                BookingStatus.Approved => "bg-success",
                BookingStatus.Rejected => "bg-danger",
                BookingStatus.Completed => "bg-info",
                BookingStatus.Cancelled => "bg-secondary",
                _ => "bg-secondary"
            };
        }
    }
}
