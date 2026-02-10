
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Pages.AllProjects;

[Authorize]
public class AllProjectsModel : PageModel
{
    private readonly ProjectService _projects;

    private const int PageSize = 5;

    public IReadOnlyList<Project> Projects { get; set; } = Array.Empty<Project>();

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalProjects { get; set; }

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public string? Search { get; set; }

    public AllProjectsModel(ProjectService projects)
    {
        _projects = projects;
    }

    public async Task OnGet(int paginator = 1, string? search = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        CurrentPage = paginator < 1 ? 1 : paginator;
        Search = search;

        var query = (await _projects.GetAllForUserAsync(userId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            query = query.Where(p =>
                p.Name.Contains(Search, StringComparison.OrdinalIgnoreCase));
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        TotalProjects = query.Count();
        TotalPages = (int)Math.Ceiling(TotalProjects / (double)PageSize);

        Projects = query
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }
}
