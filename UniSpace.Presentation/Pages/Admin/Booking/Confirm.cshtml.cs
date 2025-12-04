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

        public async Task<IActionResult> OnPostApproveAsync()
        {
            try
            {
                if (Input.Id == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "Invalid booking ID.";
                    return RedirectToPage("Index");
                }

                Booking = await _bookingService.GetBookingByIdAsync(Input.Id);

                if (Booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToPage("Index");
                }

                if (Booking.Status != BookingStatus.Pending)
                {
                    TempData["ErrorMessage"] = $"Cannot approve booking with status: {Booking.StatusDisplay}";
                    return RedirectToPage("Index");
                }

                // Validate admin note length if provided
                if (!string.IsNullOrWhiteSpace(Input.AdminNote) && Input.AdminNote.Length > 500)
                {
                    ErrorMessage = "Admin note cannot exceed 500 characters.";
                    return Page();
                }

                var success = await _bookingService.ApproveBookingAsync(Input.Id, Input.AdminNote);

                if (success)
                {
                    _logger.LogInformation($"Booking {Input.Id} approved by admin");
                    TempData["SuccessMessage"] = $"? Booking for '{Booking.RoomName}' by {Booking.UserName} has been approved successfully! User will be notified.";
                    return RedirectToPage("Index");
                }

                ErrorMessage = "Failed to approve booking. Please try again.";
                
                // Reload booking for display
                try
                {
                    Booking = await _bookingService.GetBookingByIdAsync(Input.Id);
                }
                catch
                {
                    // If reload fails, redirect
                    TempData["ErrorMessage"] = "An error occurred. Please try again.";
                    return RedirectToPage("Index");
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving booking: {Input.Id}");
                
                try
                {
                    Booking = await _bookingService.GetBookingByIdAsync(Input.Id);
                }
                catch
                {
                    TempData["ErrorMessage"] = "An error occurred. Please try again.";
                    return RedirectToPage("Index");
                }
                
                ErrorMessage = ex.Message ?? "An error occurred while approving the booking. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRejectAsync()
        {
            try
            {
                if (Input.Id == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "Invalid booking ID.";
                    return RedirectToPage("Index");
                }

                Booking = await _bookingService.GetBookingByIdAsync(Input.Id);

                if (Booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToPage("Index");
                }

                if (Booking.Status != BookingStatus.Pending)
                {
                    TempData["ErrorMessage"] = $"Cannot reject booking with status: {Booking.StatusDisplay}";
                    return RedirectToPage("Index");
                }

                // Admin note is required for rejection
                if (string.IsNullOrWhiteSpace(Input.AdminNote))
                {
                    ErrorMessage = "?? Admin note is required when rejecting a booking. Please provide a clear reason for the user.";
                    
                    // Reload booking for display
                    try
                    {
                        Booking = await _bookingService.GetBookingByIdAsync(Input.Id);
                    }
                    catch
                    {
                        TempData["ErrorMessage"] = "An error occurred. Please try again.";
                        return RedirectToPage("Index");
                    }
                    
                    return Page();
                }

                // Validate admin note length
                if (Input.AdminNote.Length > 500)
                {
                    ErrorMessage = "Admin note cannot exceed 500 characters.";
                    
                    // Reload booking for display
                    try
                    {
                        Booking = await _bookingService.GetBookingByIdAsync(Input.Id);
                    }
                    catch
                    {
                        TempData["ErrorMessage"] = "An error occurred. Please try again.";
                        return RedirectToPage("Index");
                    }
                    
                    return Page();
                }

                // Ensure note is meaningful (at least 10 characters)
                if (Input.AdminNote.Trim().Length < 10)
                {
                    ErrorMessage = "Please provide a meaningful reason (at least 10 characters) for rejecting this booking.";
                    
                    // Reload booking for display
                    try
                    {
                        Booking = await _bookingService.GetBookingByIdAsync(Input.Id);
                    }
                    catch
                    {
                        TempData["ErrorMessage"] = "An error occurred. Please try again.";
                        return RedirectToPage("Index");
                    }
                    
                    return Page();
                }

                var success = await _bookingService.RejectBookingAsync(Input.Id, Input.AdminNote);

                if (success)
                {
                    _logger.LogInformation($"Booking {Input.Id} rejected by admin with note: {Input.AdminNote}");
                    TempData["SuccessMessage"] = $"Booking for '{Booking.RoomName}' by {Booking.UserName} has been rejected. User will be notified with your explanation.";
                    return RedirectToPage("Index");
                }

                ErrorMessage = "Failed to reject booking. Please try again.";
                
                // Reload booking for display
                try
                {
                    Booking = await _bookingService.GetBookingByIdAsync(Input.Id);
                }
                catch
                {
                    TempData["ErrorMessage"] = "An error occurred. Please try again.";
                    return RedirectToPage("Index");
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting booking: {Input.Id}");
                
                try
                {
                    Booking = await _bookingService.GetBookingByIdAsync(Input.Id);
                }
                catch
                {
                    TempData["ErrorMessage"] = "An error occurred. Please try again.";
                    return RedirectToPage("Index");
                }
                
                ErrorMessage = ex.Message ?? "An error occurred while rejecting the booking. Please try again.";
                return Page();
            }
        }
    }
}
