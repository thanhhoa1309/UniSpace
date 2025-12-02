using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Room
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IRoomService roomService,
            ILogger<DetailsModel> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        public RoomDto Room { get; set; } = null!;
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Room = await _roomService.GetRoomByIdAsync(id);

                // Success/Error messages from TempData
                if (TempData["SuccessMessage"] != null)
                {
                    SuccessMessage = TempData["SuccessMessage"]?.ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ErrorMessage = TempData["ErrorMessage"]?.ToString();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading room: {id}");
                TempData["ErrorMessage"] = "Room not found or has been deleted.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _roomService.SoftDeleteRoomAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Room deleted successfully!";
                    return RedirectToPage("Index");
                }

                TempData["ErrorMessage"] = "Failed to delete room.";
                return RedirectToPage("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting room: {id}");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("Details", new { id });
            }
        }
    }
}
