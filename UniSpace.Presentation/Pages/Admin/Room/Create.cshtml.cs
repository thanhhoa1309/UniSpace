using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Room
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IRoomService roomService,
            ICampusService campusService,
            ILogger<CreateModel> logger)
        {
            _roomService = roomService;
            _campusService = campusService;
            _logger = logger;
        }

        [BindProperty]
        public CreateRoomDto Input { get; set; } = new();

        public List<SelectListItem> CampusOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadCampusOptionsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCampusOptionsAsync();
                return Page();
            }

            try
            {
                var result = await _roomService.CreateRoomAsync(Input);

                if (result != null)
                {
                    TempData["SuccessMessage"] = $"Room '{result.Name}' created successfully!";
                    return RedirectToPage("Details", new { id = result.Id });
                }

                ErrorMessage = "Failed to create room. Please try again.";
                await LoadCampusOptionsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");

                if (ex.Data.Contains("StatusCode"))
                {
                    var statusCode = (int)ex.Data["StatusCode"]!;
                    ErrorMessage = statusCode switch
                    {
                        400 => "Invalid data. Please check your input.",
                        404 => "Campus not found.",
                        409 => ex.Message,
                        _ => "An error occurred. Please try again."
                    };
                }
                else
                {
                    ErrorMessage = ex.Message;
                }

                await LoadCampusOptionsAsync();
                return Page();
            }
        }

        private async Task LoadCampusOptionsAsync()
        {
            try
            {
                var campuses = await _campusService.GetCampusesAsync();
                CampusOptions = campuses.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == Input.CampusId
                }).ToList();

                CampusOptions.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select Campus --",
                    Selected = Input.CampusId == Guid.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading campus options");
                CampusOptions = new List<SelectListItem>();
            }
        }
    }
}
