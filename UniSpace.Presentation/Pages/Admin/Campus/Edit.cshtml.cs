using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.CampusDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Campus
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ICampusService _campusService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ICampusService campusService, ILogger<EditModel> logger)
        {
            _campusService = campusService;
            _logger = logger;
        }

        [BindProperty]
        public UpdateCampusDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public CampusDto? Campus { get; set; }

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

                Input = new UpdateCampusDto
                {
                    Id = Campus.Id,
                    Name = Campus.Name,
                    Address = Campus.Address
                };

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading campus: {id}");
                TempData["ErrorMessage"] = "Error loading campus. Please try again.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    Campus = await _campusService.GetCampusByIdAsync(Input.Id);
                }
                catch { }
                return Page();
            }

            try
            {
                var result = await _campusService.UpdateCampusAsync(Input);

                if (result != null)
                {
                    TempData["SuccessMessage"] = $"Campus '{result.Name}' updated successfully!";
                    return RedirectToPage("Details", new { id = result.Id });
                }

                ErrorMessage = "Failed to update campus. Please try again.";
                Campus = await _campusService.GetCampusByIdAsync(Input.Id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating campus: {Input.Id}");

                if (ex.Data.Contains("StatusCode"))
                {
                    var statusCode = (int)ex.Data["StatusCode"]!;
                    ErrorMessage = statusCode switch
                    {
                        400 => "Invalid data. Please check your input.",
                        404 => "Campus not found.",
                        409 => $"Campus with name '{Input.Name}' already exists.",
                        _ => "An error occurred. Please try again."
                    };
                }
                else
                {
                    ErrorMessage = ex.Message;
                }

                try
                {
                    Campus = await _campusService.GetCampusByIdAsync(Input.Id);
                }
                catch { }
                
                return Page();
            }
        }
    }
}
