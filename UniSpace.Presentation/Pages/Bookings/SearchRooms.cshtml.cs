using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Bookings
{
    [Authorize]
    public class SearchRoomsModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;

        public SearchRoomsModel(IRoomService roomService, ICampusService campusService)
        {
            _roomService = roomService;
            _campusService = campusService;
        }

        public List<RoomDto> Rooms { get; set; } = new List<RoomDto>();
        public List<SelectListItem> Campuses { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> RoomTypes { get; set; } = new List<SelectListItem>();

        [BindProperty(SupportsGet = true)]
        public Guid? CampusId { get; set; }

        [BindProperty(SupportsGet = true)]
        public RoomType? RoomType { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinCapacity { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartTime { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndTime { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadCampusesAsync();
                LoadRoomTypes();

                var result = await _roomService.GetRoomsAsync(
                    pageNumber: PageNumber,
                    pageSize: 12,
                    searchTerm: SearchTerm,
                    campusId: CampusId,
                    type: RoomType,
                    status: BookingStatus.Approved, // Only show available rooms
                    availableFrom: StartTime,
                    availableTo: EndTime);

                Rooms = result.ToList();

                // Filter by capacity if specified
                if (MinCapacity.HasValue)
                {
                    Rooms = Rooms.Where(r => r.Capacity >= MinCapacity.Value).ToList();
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading rooms: {ex.Message}";
                return Page();
            }
        }

        private async Task LoadCampusesAsync()
        {
            var campusResult = await _campusService.GetCampusesAsync(pageNumber: 1, pageSize: 100);
            Campuses = campusResult.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            Campuses.Insert(0, new SelectListItem { Value = "", Text = "All Campuses" });
        }

        private void LoadRoomTypes()
        {
            RoomTypes = Enum.GetValues(typeof(RoomType))
                .Cast<RoomType>()
                .Select(rt => new SelectListItem
                {
                    Value = ((int)rt).ToString(),
                    Text = rt.ToString()
                }).ToList();

            RoomTypes.Insert(0, new SelectListItem { Value = "", Text = "All Types" });
        }
    }
}
