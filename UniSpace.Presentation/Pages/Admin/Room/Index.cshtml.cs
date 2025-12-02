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

        public List<RoomDto> Rooms { get; set; } = new();
        public string? SearchTerm { get; set; }
        public Guid? FilterCampusId { get; set; }
        public RoomType? FilterType { get; set; }
        public BookingStatus? FilterStatus { get; set; }
        public List<SelectListItem> CampusOptions { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(
            string? search,
            Guid? campusId,
            RoomType? type,
            BookingStatus? status)
        {
            try
            {
                SearchTerm = search;
                FilterCampusId = campusId;
                FilterType = type;
                FilterStatus = status;

                // Get all rooms
                var allRooms = await _roomService.GetAllRoomsAsync();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    allRooms = await _roomService.SearchRoomsAsync(SearchTerm);
                }

                if (FilterCampusId.HasValue && FilterCampusId != Guid.Empty)
                {
                    allRooms = allRooms.Where(r => r.CampusId == FilterCampusId.Value).ToList();
                }

                if (FilterType.HasValue)
                {
                    allRooms = allRooms.Where(r => r.Type == FilterType.Value).ToList();
                }

                if (FilterStatus.HasValue)
                {
                    allRooms = allRooms.Where(r => r.CurrentStatus == FilterStatus.Value).ToList();
                }

                Rooms = allRooms;

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
                Rooms = new List<RoomDto>();
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
