using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Pages;

[Authorize]
public class BoardModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TaskService _taskService;

    public BoardModel(UserManager<ApplicationUser> userManager, TaskService taskService)
    {
        _userManager = userManager;
        _taskService = taskService;
    }

    public static readonly string[] Columns = new[]
    {
        "Open",
        "In Progress",
        "Blocked",
        "Done"
    };

    public Dictionary<string, List<TaskItem>> TasksByStatus { get; private set; } = new();
    public Dictionary<string, int> CountByStatus { get; private set; } = new();
    public int TotalTasks { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var tasks = await _taskService.GetAllTaskAsync(userId, ct);

        TotalTasks = tasks.Count;

        foreach (var col in Columns)
        {
            TasksByStatus[col] = new List<TaskItem>();
            CountByStatus[col] = 0;
        }

        foreach (var t in tasks)
        {
            var status = Normalize(t.Status);
            TasksByStatus[status].Add(t);
            CountByStatus[status]++;
        }

        return Page();
    }

    private string Normalize(string? s)
    {
        var x = (s ?? "Open").Trim().ToLowerInvariant();

        return x switch
        {
            "open"           => "Open",
            "in progress"    => "In Progress",
            "inprogress"     => "In Progress",
            "blocked"        => "Blocked",
            "done"           => "Done",
            "closed"         => "Done",
            _                => "Open"
        };
    }
}