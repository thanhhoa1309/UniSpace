using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.CampusDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Campus
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ICampusService _campusService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ICampusService campusService, ILogger<IndexModel> logger)
        {
            _campusService = campusService;
            _logger = logger;
        }

        public List<CampusDto> Campuses { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(string? search)
        {
            try
            {
                SearchTerm = search;
                
                if (!string.IsNullOrWhiteSpace(search))
                {
                    Campuses = await _campusService.GetCampusesByNameAsync(search);
                }
                else
                {
                    Campuses = await _campusService.GetAllCampusesAsync();
                }

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
                _logger.LogError(ex, "Error loading campuses");
                ErrorMessage = "Error loading campuses. Please try again.";
                Campuses = new List<CampusDto>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _campusService.SoftDeleteCampusAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Campus deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Campus not found or already deleted.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting campus: {id}");
                
                if (ex.Data.Contains("StatusCode"))
                {
                    var statusCode = (int)ex.Data["StatusCode"]!;
                    if (statusCode == 400)
                    {
                        TempData["ErrorMessage"] = ex.Message;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error deleting campus. Please try again.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred.";
                }
            }

            return RedirectToPage();
        }
    }
}
