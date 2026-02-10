using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using TaskManager.Services;
using TaskManager.Models;

namespace TaskManager.Pages;

[Authorize]
public class MyTaskModel : PageModel
{
    private readonly TaskService _tasks;
    private readonly ProjectService _projects;

    public MyTaskModel(TaskService tasks, ProjectService projects)
    {
        _tasks = tasks;
        _projects = projects;
    }

    public TaskItem? Task { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        Task = await _tasks.GetByIdAsync(id, includeProject: true);

        if (Task == null)
            return NotFound();

        return Page();
    }
    
        public async Task<IActionResult> OnPostDeleteTaskAsync(int projectId, int taskId, CancellationToken ct)
        {
            var project = await _projects.GetByIdAsync(projectId, includeUsers: false, includeIssues: true, ct);
            if (project is null) return NotFound();

            var task = project.Tasks?.FirstOrDefault(t => t.Id == taskId);
            if (task is null || task.ProjectId != projectId)
            {
                TempData["Error"] = "La tarea no existe o no pertenece a este proyecto.";
                return RedirectToPage(new { id = projectId });
            }

            var ok = await _tasks.DeleteAsync(taskId, ct);
            TempData[ ok ? "Success" : "Error" ] = ok ? "Tarea eliminada." : "No se pudo eliminar la tarea.";
            return RedirectToPage(new { id = projectId });
        }
}
