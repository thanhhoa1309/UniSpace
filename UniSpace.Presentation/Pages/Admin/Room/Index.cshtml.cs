using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;
using UniSpace.Services.Utils;

namespace UniSpace.Presentation.Pages.Admin.Room
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IRoomService roomService,
            ICampusService campusService,
            ILogger<IndexModel> logger)
        {
            _roomService = roomService;
            _campusService = campusService;
            _logger = logger;
        }

        public Pagination<RoomDto> Rooms { get; set; } = new Pagination<RoomDto>(new List<RoomDto>(), 0, 1, 20);
        public string? SearchTerm { get; set; }
        public Guid? FilterCampusId { get; set; }
        public RoomType? FilterType { get; set; }
        public BookingStatus? FilterStatus { get; set; }
        public List<SelectListItem> CampusOptions { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            Guid? campusId = null,
            RoomType? type = null,
            BookingStatus? status = null)
        {
            try
            {
                SearchTerm = search;
                FilterCampusId = campusId;
                FilterType = type;
                FilterStatus = status;

                // Use the unified GetRoomsAsync method with all filters
                Rooms = await _roomService.GetRoomsAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    searchTerm: search,
                    campusId: campusId,
                    type: type,
                    status: status);

                // Load campus options for filter
                var campuses = await _campusService.GetAllCampusesAsync();
                CampusOptions = campuses.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == FilterCampusId
                }).ToList();

                CampusOptions.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "All Campuses",
                    Selected = !FilterCampusId.HasValue
                });

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
                _logger.LogError(ex, "Error loading rooms");
                ErrorMessage = "Error loading rooms. Please try again.";
                Rooms = new Pagination<RoomDto>(new List<RoomDto>(), 0, 1, 20);
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
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete room.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting room: {id}");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }
    }
}
