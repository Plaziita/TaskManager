using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Pages.MyProject
{
    [Authorize]
    public class MyProjectModel : PageModel
    {
        private readonly ProjectService _projects;
        private readonly TaskService _tasks;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyProjectModel(
            ProjectService projects,
            TaskService tasks,
            UserManager<ApplicationUser> userManager)
        {
            _projects = projects;
            _tasks = tasks;
            _userManager = userManager;
        }

        public Project? Current { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
        {
            Current = await _projects.GetByIdAsync(
                id,
                includeUsers: true,
                includeIssues: true,
                ct: ct);

            if (Current is null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteProjectAsync(int projectId, CancellationToken ct)
        {
            var ok = await _projects.DeleteAsync(projectId, ct);
            if (!ok)
            {
                TempData["Error"] = "No se pudo eliminar el proyecto.";
                return RedirectToPage(new { id = projectId });
            }

            return RedirectToPage("/Dashboard/Dashboard");
        }

        public async Task<IActionResult> OnPostRemoveMemberAsync(int id, string userId, CancellationToken ct)
        {
            var ok = await _projects.RemoveUserAsync(id, userId, ct);
            TempData[ ok ? "Success" : "Error" ] = ok ? "Miembro eliminado." : "No se pudo eliminar.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAddMemberAsync(int id, string email, CancellationToken ct)
        {
            email = (email ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "El email es obligatorio.";
                return RedirectToPage(new { id });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                TempData["Error"] = $"No existe ningún usuario con el email '{email}'.";
                return RedirectToPage(new { id });
            }

            var project = await _projects.GetByIdAsync(id, includeUsers: true, includeIssues: false, ct);
            if (project is null) return NotFound();

            if (project.Users.Any(u => u.Id == user.Id))
            {
                TempData["Info"] = $"'{user.UserName}' ya es miembro del proyecto.";
                return RedirectToPage(new { id });
            }

            var ok = await _projects.AddUsersAsync(id, new[] { user.Id }, ct);
            TempData[ ok ? "Success" : "Error" ] = ok
                ? $"'{user.UserName}' ha sido añadido al proyecto."
                : "No se pudo añadir el usuario al proyecto.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostEditProjectAsync(
            int projectId,
            string name,
            string? description,
            DateTime? startDate,
            DateTime? dueDate,
            CancellationToken ct)
        {
            name = (name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "El nombre de proyecto es obligatorio.";
                return RedirectToPage(new { id = projectId });
            }

            var updated = new Project
            {
                Id = projectId,
                Name = name,
                Description = description,
                StartDate = startDate,
                DueDate = dueDate
            };

            await _projects.UpdateAsync(updated, ct);
            TempData["Success"] = "Proyecto actualizado.";
            return RedirectToPage(new { id = projectId });
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

        public async Task<IActionResult> OnPostEditTaskAsync(
            int projectId,
            int taskId,
            string title,
            string status,
            string? assignedUserId,
            CancellationToken ct)
        {
            title = (title ?? string.Empty).Trim();
            status = (status ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "El título es obligatorio.";
                return RedirectToPage(new { id = projectId });
            }

            var project = await _projects.GetByIdAsync(projectId, includeUsers: true, includeIssues: true, ct);
            if (project is null)
                return NotFound();

            var existing = project.Tasks?.FirstOrDefault(t => t.Id == taskId);
            if (existing is null || existing.ProjectId != projectId)
            {
                TempData["Error"] = "La tarea no existe o no pertenece a este proyecto.";
                return RedirectToPage(new { id = projectId });
            }

            if (!string.IsNullOrWhiteSpace(assignedUserId))
            {
                var isMember = project.Users.Any(u => u.Id == assignedUserId);
                if (!isMember)
                {
                    TempData["Error"] = "Solo puedes asignar miembros del proyecto.";
                    return RedirectToPage(new { id = projectId });
                }
            }

            var updated = new TaskItem
            {
                Id = taskId,
                Title = title,
                Status = status,
                Description = existing.Description,
                Priority = existing.Priority,
                DueDate = existing.DueDate,
                ProjectId = existing.ProjectId,
                AssignedUserId = existing.AssignedUserId,
                CreatedAt = existing.CreatedAt
            };

            await _tasks.UpdateAsync(updated, ct);

            if ((assignedUserId ?? string.Empty) != (existing.AssignedUserId ?? string.Empty))
            {
                await _tasks.AssignUserAsync(taskId, string.IsNullOrWhiteSpace(assignedUserId) ? null : assignedUserId, ct);
            }

            TempData["Success"] = "Tarea actualizada.";
            return RedirectToPage(new { id = projectId });
        }
    }
}