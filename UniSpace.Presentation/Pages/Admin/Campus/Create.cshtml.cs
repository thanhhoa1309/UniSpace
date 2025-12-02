using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.CampusDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Campus
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ICampusService _campusService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ICampusService campusService, ILogger<CreateModel> logger)
        {
            _campusService = campusService;
            _logger = logger;
        }

        [BindProperty]
        public CreateCampusDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _campusService.CreateCampusAsync(Input);

                if (result != null)
                {
                    TempData["SuccessMessage"] = $"Campus '{result.Name}' created successfully!";
                    return RedirectToPage("Index");
                }

                ErrorMessage = "Failed to create campus. Please try again.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating campus");

                if (ex.Data.Contains("StatusCode"))
                {
                    var statusCode = (int)ex.Data["StatusCode"]!;
                    ErrorMessage = statusCode switch
                    {
                        400 => "Invalid data. Please check your input.",
                        409 => $"Campus with name '{Input.Name}' already exists.",
                        _ => "An error occurred. Please try again."
                    };
                }
                else
                {
                    ErrorMessage = ex.Message;
                }

                return Page();
            }
        }
    }
}
