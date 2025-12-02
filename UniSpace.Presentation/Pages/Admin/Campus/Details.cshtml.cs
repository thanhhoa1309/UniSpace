using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.CampusDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Campus
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ICampusService _campusService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ICampusService campusService, ILogger<DetailsModel> logger)
        {
            _campusService = campusService;
            _logger = logger;
        }

        public CampusDto? Campus { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Campus = await _campusService.GetCampusByIdAsync(id);

                if (Campus == null)
                {
                    TempData["ErrorMessage"] = "Campus not found.";
                    return RedirectToPage("Index");
                }

                if (TempData["SuccessMessage"] != null)
                {
                    SuccessMessage = TempData["SuccessMessage"]?.ToString();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading campus details: {id}");
                TempData["ErrorMessage"] = "Error loading campus details. Please try again.";
                return RedirectToPage("Index");
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
                    return RedirectToPage("Index");
                }

                TempData["ErrorMessage"] = "Failed to delete campus.";
                return RedirectToPage("Details", new { id });
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
                    TempData["ErrorMessage"] = ex.Message;
                }

                return RedirectToPage("Details", new { id });
            }
        }
    }
}
