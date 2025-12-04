using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;
using UniSpace.Services.Utils;

namespace UniSpace.Presentation.Pages.Booking
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IBookingService bookingService, ILogger<IndexModel> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        public Pagination<BookingDto> Bookings { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                BookingStatus? statusEnum = null;
                if (!string.IsNullOrEmpty(Status) && Enum.TryParse<BookingStatus>(Status, out var parsed))
                {
                    statusEnum = parsed;
                }

                // Get current user's bookings only
                var myBookings = await _bookingService.GetMyBookingsAsync();

                // Apply filters manually
                var filtered = myBookings.AsEnumerable();

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    filtered = filtered.Where(b =>
                        b.RoomName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.Purpose.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
                }

                if (statusEnum.HasValue)
                {
                    filtered = filtered.Where(b => b.Status == statusEnum.Value);
                }

                if (StartDate.HasValue)
                {
                    filtered = filtered.Where(b => b.StartTime.Date >= StartDate.Value.Date);
                }

                var filteredList = filtered.ToList();
                var totalCount = filteredList.Count;
                var paginatedList = filteredList
                    .Skip((PageNumber - 1) * 10)
                    .Take(10)
                    .ToList();

                Bookings = new Pagination<BookingDto>(paginatedList, totalCount, PageNumber, 10);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bookings");
                TempData["ErrorMessage"] = "Error loading bookings. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            try
            {
                await _bookingService.CancelBookingAsync(id);
                TempData["SuccessMessage"] = "Booking cancelled successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }
    }
}
