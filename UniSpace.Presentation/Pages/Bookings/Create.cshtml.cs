using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Bookings
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly IScheduleService _scheduleService;

        public CreateModel(IBookingService bookingService, IRoomService roomService, IScheduleService scheduleService)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _scheduleService = scheduleService;
        }

        [BindProperty]
        public CreateBookingDto Booking { get; set; } = new CreateBookingDto();

        public RoomDto? Room { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid roomId, DateTime? startTime = null, DateTime? endTime = null)
        {
            try
            {
                Room = await _roomService.GetRoomByIdAsync(roomId);
                if (Room == null)
                {
                    TempData["Error"] = "Room not found.";
                    return RedirectToPage("./SearchRooms");
                }

                Booking.RoomId = roomId;

                // Pre-fill times if provided
                if (startTime.HasValue)
                {
                    Booking.StartTime = startTime.Value;
                }
                else
                {
                    // Default to tomorrow at 9 AM
                    Booking.StartTime = DateTime.Now.Date.AddDays(1).AddHours(9);
                }

                if (endTime.HasValue)
                {
                    Booking.EndTime = endTime.Value;
                }
                else
                {
                    // Default to 1 hour after start time
                    Booking.EndTime = Booking.StartTime.AddHours(1);
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading room: {ex.Message}";
                return RedirectToPage("./SearchRooms");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Room = await _roomService.GetRoomByIdAsync(Booking.RoomId);
                    return Page();
                }

                // Additional validation
                if (Booking.StartTime >= Booking.EndTime)
                {
                    ModelState.AddModelError("Booking.EndTime", "End time must be after start time.");
                    Room = await _roomService.GetRoomByIdAsync(Booking.RoomId);
                    return Page();
                }

                if (Booking.StartTime < DateTime.Now)
                {
                    ModelState.AddModelError("Booking.StartTime", "Cannot book rooms in the past.");
                    Room = await _roomService.GetRoomByIdAsync(Booking.RoomId);
                    return Page();
                }

                var result = await _bookingService.CreateBookingAsync(Booking);

                if (result != null)
                {
                    TempData["Success"] = "Booking created successfully! Your booking is pending approval.";
                    return RedirectToPage("./MyBookings");
                }
                else
                {
                    TempData["Error"] = "Failed to create booking.";
                    Room = await _roomService.GetRoomByIdAsync(Booking.RoomId);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Room = await _roomService.GetRoomByIdAsync(Booking.RoomId);
                return Page();
            }
        }

        public async Task<IActionResult> OnGetCheckAvailabilityAsync(Guid roomId, DateTime startTime, DateTime endTime)
        {
            try
            {
                var isAvailable = await _bookingService.IsRoomAvailableForBookingAsync(roomId, startTime, endTime);
                return new JsonResult(new { available = isAvailable });
            }
            catch
            {
                return new JsonResult(new { available = false });
            }
        }
    }
}
