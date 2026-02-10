using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Pages.CreateTask;

[Authorize]
public class CreateTaskModel : PageModel
{
    private readonly ProjectService _projects;
    private readonly TaskService _tasks;

    public CreateTaskModel(ProjectService projects, TaskService tasks)
    {
        _projects = projects;
        _tasks = tasks;
    }

    public int ProjectId { get; private set; }

    [BindProperty] public string Title { get; set; } = string.Empty;
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public string Status { get; set; } = "Open";
    [BindProperty] public int Priority { get; set; } = 3;
    [BindProperty] public DateTime? DueDate { get; set; }
    [BindProperty] public string? AssignedUserId { get; set; }

    public List<ApplicationUser> ProjectUsers { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int projectId, CancellationToken ct)
    {
        ProjectId = projectId;

        var project = await _projects.GetProjectWithUsersAsync(projectId, ct);

        if (project is null)
            return NotFound();

        ProjectUsers = project.Users.ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int projectId, CancellationToken ct)
    {
        ProjectId = projectId;

        if (!ModelState.IsValid)
        {
            var project = await _projects.GetProjectWithUsersAsync(projectId, ct);
            ProjectUsers = project?.Users.ToList() ?? new List<ApplicationUser>();
            return Page();
        }

        var task = new TaskItem
        {
            Title = Title.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            Status = Status,
            Priority = Priority,
            DueDate = DueDate,
            AssignedUserId = AssignedUserId
        };

        await _tasks.CreateAsync(ProjectId, task, ct);

        return RedirectToPage("/MyProject/MyProject", new { id = ProjectId });
    }
}