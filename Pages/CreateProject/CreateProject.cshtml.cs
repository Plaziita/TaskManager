
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Pages.CreateProject;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ProjectService _projects;

    public CreateModel(ProjectService projects)
    {
        _projects = projects;
    }

    [BindProperty] public string Name { get; set; } = string.Empty;
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public DateTime? StartDate { get; set; }
    [BindProperty] public DateTime? DueDate { get; set; }

    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        return NotFound();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ModelState.AddModelError(nameof(Name), "Project name is required.");
        }
        if (!ModelState.IsValid)
        {
            return LocalRedirect(GetSafeReturnUrl() ?? Url.Page("/Dashboard")!);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var project = new Project
        {
            Name = Name,
            Description = Description,
            StartDate = StartDate,
            DueDate = DueDate
        };

        await _projects.CreateAsync(project, new[] { userId });

        var back = GetSafeReturnUrl() ?? Url.Page("/Dashboard/Dashboard");
        return LocalRedirect(back!);
    }

    private string? GetSafeReturnUrl()
    {
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return ReturnUrl;

        return null;
    }
}
