using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Booking
{
    [Authorize(Roles = "Admin")]
    public class ConfirmModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<ConfirmModel> _logger;

        public ConfirmModel(
            IBookingService bookingService,
            ILogger<ConfirmModel> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        public BookingDto? Booking { get; set; }
        
        [BindProperty]
        public ConfirmBookingDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id = null)
        {
            try
            {
                // Check if ID is provided
                if (!id.HasValue || id.Value == Guid.Empty)
                {
                    _logger.LogWarning("Confirm page accessed without valid booking ID");
                    TempData["ErrorMessage"] = "Booking ID is required. Please select a booking from the list.";
                    return RedirectToPage("Index");
                }

                // Load booking
                Booking = await _bookingService.GetBookingByIdAsync(id.Value);

                if (Booking == null)
                {
                    _logger.LogWarning($"Booking not found: {id.Value}");
                    TempData["ErrorMessage"] = "Booking not found. It may have been deleted or you don't have permission to view it.";
                    return RedirectToPage("Index");
                }

                // Only pending bookings can be confirmed
                if (Booking.Status != BookingStatus.Pending)
                {
                    _logger.LogWarning($"Attempted to confirm non-pending booking: {id.Value}, Status: {Booking.Status}");
                    TempData["ErrorMessage"] = $"Cannot confirm booking with status '{Booking.StatusDisplay}'. Only pending bookings can be confirmed.";
                    return RedirectToPage("Index");
                }

                // Check if booking time has already passed
                if (Booking.StartTime < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Attempted to confirm past booking: {id.Value}, StartTime: {Booking.StartTime}");
                    TempData["ErrorMessage"] = "Cannot confirm a booking that has already started or passed.";
                    return RedirectToPage("Index");
                }

                Input.Id = Booking.Id;
                _logger.LogInformation($"Booking {id.Value} loaded for confirmation by admin");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading booking for confirmation: {id}");
                TempData["ErrorMessage"] = "An error occurred while loading the booking. Please try again.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostConfirmAsync()
        {
            try
            {
                if (Input.Id == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "Invalid booking ID.";
                    return RedirectToPage("Index");
                }

                // Validate status
                if (Input.Status != BookingStatus.Approved && Input.Status != BookingStatus.Rejected)
                {
                    TempData["ErrorMessage"] = "Invalid action. Please use Approve or Reject buttons.";
                    return RedirectToPage("Index");
                }

                // Load booking first
                Booking = await _bookingService.GetBookingByIdAsync(Input.Id);

                if (Booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToPage("Index");
                }

                if (Booking.Status != BookingStatus.Pending)
                {
                    TempData["ErrorMessage"] = $"Cannot confirm booking with status: {Booking.StatusDisplay}";
                    return RedirectToPage("Index");
                }

                // Validate admin note for rejection
                if (Input.Status == BookingStatus.Rejected)
                {
                    if (string.IsNullOrWhiteSpace(Input.AdminNote))
                    {
                        ErrorMessage = "?? Admin note is required when rejecting a booking. Please provide a clear reason for the user.";
                        return Page();
                    }

                    if (Input.AdminNote.Trim().Length < 10)
                    {
                        ErrorMessage = "Please provide a meaningful reason (at least 10 characters) for rejecting this booking.";
                        return Page();
                    }
                }

                // Validate admin note length
                if (!string.IsNullOrWhiteSpace(Input.AdminNote) && Input.AdminNote.Length > 500)
                {
                    ErrorMessage = "Admin note cannot exceed 500 characters.";
                    return Page();
                }

                // Use unified confirm method
                var result = await _bookingService.ConfirmBookingAsync(Input);

                if (result != null)
                {
                    var actionText = Input.Status == BookingStatus.Approved ? "approved" : "rejected";
                    var icon = Input.Status == BookingStatus.Approved ? "?" : "?";
                    
                    _logger.LogInformation($"Booking {Input.Id} {actionText} by admin");
                    TempData["SuccessMessage"] = $"{icon} Booking for '{Booking.RoomName}' by {Booking.UserName} has been {actionText} successfully! User will be notified via real-time notification.";
                    return RedirectToPage("Index");
                }

                ErrorMessage = "Failed to confirm booking. Please try again.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming booking: {Input.Id}");
                
                try
                {
                    Booking = await _bookingService.GetBookingByIdAsync(Input.Id);
                }
                catch
                {
                    TempData["ErrorMessage"] = "An error occurred. Please try again.";
                    return RedirectToPage("Index");
                }
                
                ErrorMessage = ex.Message ?? "An error occurred while confirming the booking. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            // Set status and call unified method
            Input.Status = BookingStatus.Approved;
            return await OnPostConfirmAsync();
        }

        public async Task<IActionResult> OnPostRejectAsync()
        {
            // Set status and call unified method
            Input.Status = BookingStatus.Rejected;
            return await OnPostConfirmAsync();
        }
    }
}
