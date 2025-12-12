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
                _logger.LogInformation($"[OnGetAsync] Starting - BookingId: {id}");

                if (!id.HasValue || id.Value == Guid.Empty)
                {
                    _logger.LogWarning("[OnGetAsync] ERROR: Booking ID is null or empty");
                    _logger.LogWarning($"[OnGetAsync] id.HasValue: {id.HasValue}, id.Value: {(id.HasValue ? id.Value.ToString() : "null")}");
                    TempData["ErrorMessage"] = "Booking ID is required. Please select a booking from the list.";
                    return RedirectToPage("Index");
                }

                _logger.LogInformation($"[OnGetAsync] Fetching booking from database - BookingId: {id.Value}");

                Booking = await _bookingService.GetBookingByIdAsync(id.Value);

                if (Booking == null)
                {
                    _logger.LogWarning($"[OnGetAsync] ERROR: Booking not found in database - BookingId: {id.Value}");
                    TempData["ErrorMessage"] = "Booking not found. It may have been deleted or you don't have permission to view it.";
                    return RedirectToPage("Index");
                }

                _logger.LogInformation($"[OnGetAsync] Booking loaded - BookingId: {id.Value}, Status: {Booking.Status}, StartTime: {Booking.StartTime}");

                if (Booking.Status != BookingStatus.Pending)
                {
                    _logger.LogWarning($"[OnGetAsync] ERROR: Invalid booking status - BookingId: {id.Value}, CurrentStatus: {Booking.Status}, StatusDisplay: {Booking.StatusDisplay}");
                    _logger.LogWarning($"[OnGetAsync] Expected Status: Pending, Got: {Booking.Status}");
                    TempData["ErrorMessage"] = $"Cannot confirm booking with status '{Booking.StatusDisplay}'. Only pending bookings can be confirmed.";
                    return RedirectToPage("Index");
                }

                if (Booking.StartTime < DateTime.UtcNow)
                {
                    _logger.LogWarning($"[OnGetAsync] ERROR: Booking time has passed - BookingId: {id.Value}, StartTime: {Booking.StartTime}, CurrentTime: {DateTime.UtcNow}");
                    _logger.LogWarning($"[OnGetAsync] Time difference: {(DateTime.UtcNow - Booking.StartTime).TotalMinutes} minutes ago");
                    TempData["ErrorMessage"] = "Cannot confirm a booking that has already started or passed.";
                    return RedirectToPage("Index");
                }

                Input.Id = Booking.Id;
                _logger.LogInformation($"[OnGetAsync] SUCCESS - Booking loaded for confirmation - BookingId: {id.Value}, Room: {Booking.RoomName}, User: {Booking.UserName}");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[OnGetAsync] EXCEPTION occurred - BookingId: {id}");
                _logger.LogError($"[OnGetAsync] Exception Type: {ex.GetType().Name}");
                _logger.LogError($"[OnGetAsync] Exception Message: {ex.Message}");
                _logger.LogError($"[OnGetAsync] Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"[OnGetAsync] Inner Exception: {ex.InnerException.Message}");
                }
                TempData["ErrorMessage"] = "An error occurred while loading the booking. Please try again.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostConfirmAsync()
        {
            try
            {
                _logger.LogInformation($"[OnPostConfirmAsync] Starting - BookingId: {Input.Id}, Status: {Input.Status}");

                if (Input.Id == Guid.Empty)
                {
                    _logger.LogWarning("[OnPostConfirmAsync] ERROR: Booking ID is empty");
                    TempData["ErrorMessage"] = "Invalid booking ID.";
                    return RedirectToPage("Index");
                }

                if (Input.Status != BookingStatus.Approved && Input.Status != BookingStatus.Rejected)
                {
                    _logger.LogWarning($"[OnPostConfirmAsync] ERROR: Invalid status - BookingId: {Input.Id}, Status: {Input.Status}");
                    _logger.LogWarning($"[OnPostConfirmAsync] Expected: Approved or Rejected, Got: {Input.Status}");
                    TempData["ErrorMessage"] = "Invalid action. Please use Approve or Reject buttons.";
                    return RedirectToPage("Index");
                }

                _logger.LogInformation($"[OnPostConfirmAsync] Fetching booking from database - BookingId: {Input.Id}");

                Booking = await _bookingService.GetBookingByIdAsync(Input.Id);

                if (Booking == null)
                {
                    _logger.LogWarning($"[OnPostConfirmAsync] ERROR: Booking not found - BookingId: {Input.Id}");
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToPage("Index");
                }

                _logger.LogInformation($"[OnPostConfirmAsync] Booking loaded - BookingId: {Input.Id}, CurrentStatus: {Booking.Status}");

                if (Booking.Status != BookingStatus.Pending)
                {
                    _logger.LogWarning($"[OnPostConfirmAsync] ERROR: Cannot confirm non-pending booking - BookingId: {Input.Id}, CurrentStatus: {Booking.Status}, StatusDisplay: {Booking.StatusDisplay}");
                    TempData["ErrorMessage"] = $"Cannot confirm booking with status: {Booking.StatusDisplay}";
                    return RedirectToPage("Index");
                }

                if (Input.Status == BookingStatus.Rejected)
                {
                    _logger.LogInformation($"[OnPostConfirmAsync] Validating rejection - AdminNote length: {Input.AdminNote?.Length ?? 0}");

                    if (string.IsNullOrWhiteSpace(Input.AdminNote))
                    {
                        _logger.LogWarning($"[OnPostConfirmAsync] ERROR: Admin note is missing for rejection - BookingId: {Input.Id}");
                        ErrorMessage = "⚠️ Admin note is required when rejecting a booking. Please provide a clear reason for the user.";
                        return Page();
                    }

                    if (Input.AdminNote.Trim().Length < 10)
                    {
                        _logger.LogWarning($"[OnPostConfirmAsync] ERROR: Admin note too short - BookingId: {Input.Id}, Length: {Input.AdminNote.Trim().Length}");
                        ErrorMessage = "Please provide a meaningful reason (at least 10 characters) for rejecting this booking.";
                        return Page();
                    }
                }

                if (!string.IsNullOrWhiteSpace(Input.AdminNote) && Input.AdminNote.Length > 500)
                {
                    _logger.LogWarning($"[OnPostConfirmAsync] ERROR: Admin note too long - BookingId: {Input.Id}, Length: {Input.AdminNote.Length}");
                    ErrorMessage = "Admin note cannot exceed 500 characters.";
                    return Page();
                }

                _logger.LogInformation($"[OnPostConfirmAsync] Calling BookingService.ConfirmBookingAsync - BookingId: {Input.Id}, Status: {Input.Status}");

                var result = await _bookingService.ConfirmBookingAsync(Input);

                if (result != null)
                {
                    var actionText = Input.Status == BookingStatus.Approved ? "approved" : "rejected";
                    var icon = Input.Status == BookingStatus.Approved ? "✅" : "❌";

                    _logger.LogInformation($"[OnPostConfirmAsync] SUCCESS - Booking {actionText} - BookingId: {Input.Id}, Room: {Booking.RoomName}, User: {Booking.UserName}");
                    TempData["SuccessMessage"] = $"{icon} Booking for '{Booking.RoomName}' by {Booking.UserName} has been {actionText} successfully! User will be notified via real-time notification.";
                    return RedirectToPage("Index");
                }

                _logger.LogWarning($"[OnPostConfirmAsync] ERROR: BookingService returned null - BookingId: {Input.Id}");
                ErrorMessage = "Failed to confirm booking. Please try again.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[OnPostConfirmAsync] EXCEPTION occurred - BookingId: {Input.Id}, Status: {Input.Status}");
                _logger.LogError($"[OnPostConfirmAsync] Exception Type: {ex.GetType().Name}");
                _logger.LogError($"[OnPostConfirmAsync] Exception Message: {ex.Message}");
                _logger.LogError($"[OnPostConfirmAsync] Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"[OnPostConfirmAsync] Inner Exception: {ex.InnerException.Message}");
                }

                try
                {
                    _logger.LogInformation($"[OnPostConfirmAsync] Attempting to reload booking after exception - BookingId: {Input.Id}");
                    Booking = await _bookingService.GetBookingByIdAsync(Input.Id);
                }
                catch (Exception reloadEx)
                {
                    _logger.LogError(reloadEx, $"[OnPostConfirmAsync] EXCEPTION while reloading booking - BookingId: {Input.Id}");
                    _logger.LogError($"[OnPostConfirmAsync] Reload Exception Type: {reloadEx.GetType().Name}");
                    _logger.LogError($"[OnPostConfirmAsync] Reload Exception Message: {reloadEx.Message}");
                    TempData["ErrorMessage"] = "An error occurred. Please try again.";
                    return RedirectToPage("Index");
                }

                ErrorMessage = ex.Message ?? "An error occurred while confirming the booking. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            _logger.LogInformation($"[OnPostApproveAsync] Approve button clicked - BookingId: {Input.Id}");
            Input.Status = BookingStatus.Approved;
            return await OnPostConfirmAsync();
        }

        public async Task<IActionResult> OnPostRejectAsync()
        {
            _logger.LogInformation($"[OnPostRejectAsync] Reject button clicked - BookingId: {Input.Id}");
            Input.Status = BookingStatus.Rejected;
            return await OnPostConfirmAsync();
        }
    }
}