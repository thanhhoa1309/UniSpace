using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Room
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IRoomService roomService,
            ICampusService campusService,
            ILogger<EditModel> logger)
        {
            _roomService = roomService;
            _campusService = campusService;
            _logger = logger;
        }

        [BindProperty]
        public UpdateRoomDto Input { get; set; } = new();

        public List<SelectListItem> CampusOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public RoomDto? CurrentRoom { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                CurrentRoom = await _roomService.GetRoomByIdAsync(id);
                
                // Populate Input with current values
                Input = new UpdateRoomDto
                {
                    Id = CurrentRoom.Id,
                    CampusId = CurrentRoom.CampusId,
                    Name = CurrentRoom.Name,
                    Type = CurrentRoom.Type,
                    Capacity = CurrentRoom.Capacity,
                    CurrentStatus = CurrentRoom.CurrentStatus,
                    Description = CurrentRoom.Description
                };

                await LoadCampusOptionsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading room: {id}");
                TempData["ErrorMessage"] = "Room not found or has been deleted.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentRoom = await _roomService.GetRoomByIdAsync(Input.Id);
                await LoadCampusOptionsAsync();
                return Page();
            }

            try
            {
                var result = await _roomService.UpdateRoomAsync(Input);

                if (result != null)
                {
                    TempData["SuccessMessage"] = $"Room '{result.Name}' updated successfully!";
                    return RedirectToPage("Details", new { id = result.Id });
                }

                ErrorMessage = "Failed to update room. Please try again.";
                CurrentRoom = await _roomService.GetRoomByIdAsync(Input.Id);
                await LoadCampusOptionsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room");

                if (ex.Data.Contains("StatusCode"))
                {
                    var statusCode = (int)ex.Data["StatusCode"]!;
                    ErrorMessage = statusCode switch
                    {
                        400 => "Invalid data. Please check your input.",
                        404 => "Room or Campus not found.",
                        409 => ex.Message,
                        _ => "An error occurred. Please try again."
                    };
                }
                else
                {
                    ErrorMessage = ex.Message;
                }

                CurrentRoom = await _roomService.GetRoomByIdAsync(Input.Id);
                await LoadCampusOptionsAsync();
                return Page();
            }
        }

        private async Task LoadCampusOptionsAsync()
        {
            try
            {
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusOptions = campuses.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == Input.CampusId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading campus options");
                CampusOptions = new List<SelectListItem>();
            }
        }
    }
}
