
using TaskManager.Models;
using TaskManager.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Pages;

public class DashboardModel : PageModel
{
    private readonly ProjectService _projects;
    private readonly TaskService _tasks;
    public DashboardModel(ProjectService projects, TaskService tasks)
    {
        _projects = projects;
        _tasks = tasks;
    }

    public IReadOnlyList<Project> Projects { get; set; } = Array.Empty<Project>();
    public IReadOnlyList<TaskItem> RecentTasks { get; private set; } = Array.Empty<TaskItem>();
    public int DoneTask { get; private set; }
    public int InProgressTask { get; private set; } 
    public int OverdueTask { get; private set; }
    public double CompletationRate {get; private set; }


    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public DateTime? StartDate { get; set; }

    [BindProperty]
    public DateTime? DueDate { get; set; }

    public async Task OnGet()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        Projects = (await _projects.GetAllForUserAsync(userId))
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .ToList();

        
        RecentTasks = await _tasks.getTaskByRecentDateAsync(userId);
        DoneTask = await _tasks.getCompletedTaskAsync(userId);
        InProgressTask = await _tasks.getInProgressTaskAsync(userId);
        OverdueTask = await _tasks.getOverdueTask(userId);
        CompletationRate = calculateRate(DoneTask, InProgressTask);
    }

    public static double calculateRate(int done, int total)
    {
        return Math.Round(100.0 * done / total, 1);
    }

    public async Task<IActionResult> OnPostDeleteProjectAsync(int projectId)
    {
        await _projects.DeleteAsync(projectId);
        return RedirectToPage();
    }
}
