using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;
using UniSpace.Services.Utils;

namespace UniSpace.Presentation.Pages.Admin.Booking
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IBookingService bookingService,
            ILogger<IndexModel> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        public Pagination<BookingDto> Bookings { get; set; } = new Pagination<BookingDto>(new List<BookingDto>(), 0, 1, 20);
        public string? SearchTerm { get; set; }
        public BookingStatus? FilterStatus { get; set; }
        public DateTime? FilterFromDate { get; set; }
        public DateTime? FilterToDate { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            BookingStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                SearchTerm = search;
                FilterStatus = status;
                FilterFromDate = fromDate;
                FilterToDate = toDate;

                // Get bookings with filters
                Bookings = await _bookingService.GetBookingsAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    searchTerm: search,
                    status: status,
                    fromDate: fromDate,
                    toDate: toDate);

                // Success/Error messages from TempData
                if (TempData["SuccessMessage"] != null)
                {
                    SuccessMessage = TempData["SuccessMessage"]?.ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ErrorMessage = TempData["ErrorMessage"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bookings");
                ErrorMessage = "Error loading bookings. Please try again.";
                Bookings = new Pagination<BookingDto>(new List<BookingDto>(), 0, 1, 20);
            }
        }

        public async Task<IActionResult> OnPostQuickApproveAsync(Guid id)
        {
            try
            {
                var success = await _bookingService.ApproveBookingAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Booking approved successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve booking.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving booking: {id}");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _bookingService.SoftDeleteBookingAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Booking deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete booking.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting booking: {id}");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
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
