
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class ProjectService
    {
        private readonly AppDbContext _db;

        public ProjectService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Project> CreateAsync(Project project, IEnumerable<string>? userIds = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(project.Name))
                throw new ArgumentException("Project name is required.", nameof(project.Name));

            if (userIds is not null)
            {
                var users = await _db.Users.Where(u => userIds.Contains(u.Id)).ToListAsync(ct);
                foreach (var u in users)
                    project.Users.Add(u);
            }

            _db.Projects.Add(project);
            await _db.SaveChangesAsync(ct);
            return project;
        }

        
        
        public async Task<Project?> GetByIdAsync(int id, bool includeUsers = true, bool includeIssues = true, CancellationToken ct = default)
        {
            IQueryable<Project> q = _db.Projects.AsQueryable();
            if (includeUsers) q = q.Include(p => p.Users);
            if (includeIssues) q = q
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedUser); 

            return await q.FirstOrDefaultAsync(p => p.Id == id, ct);
        }



        public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Projects
                .Include(p => p.Tasks)
                .Include(p => p.Users)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }
        
        public async Task<IReadOnlyList<Project>> GetAllForUserAsync(string userId, CancellationToken ct = default)
        {
            return await _db.Projects
                .Where(p => p.Users.Any(u => u.Id == userId))
                .Include(p => p.Tasks)
                .Include(p => p.Users)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }


        public async Task<Project> UpdateAsync(Project project, CancellationToken ct = default)
        {
            var existing = await _db.Projects.FirstOrDefaultAsync(p => p.Id == project.Id, ct)
                           ?? throw new KeyNotFoundException("Project not found.");

            existing.Name = project.Name;
            existing.Description = project.Description;
            existing.StartDate = project.StartDate;
            existing.DueDate = project.DueDate;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Projects.FindAsync([id], ct);
            if (existing is null) return false;
            _db.Projects.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> AddUsersAsync(int projectId, IEnumerable<string> userIds, CancellationToken ct = default)
        {
            var project = await _db.Projects.Include(p => p.Users).FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project is null) return false;

            var users = await _db.Users.Where(u => userIds.Contains(u.Id)).ToListAsync(ct);
            foreach (var u in users)
                if (!project.Users.Any(x => x.Id == u.Id))
                    project.Users.Add(u);

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RemoveUserAsync(int projectId, string userId, CancellationToken ct = default)
        {
            var project = await _db.Projects.Include(p => p.Users).FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project is null) return false;

            var toRemove = project.Users.FirstOrDefault(u => u.Id == userId);
            if (toRemove is null) return false;

            project.Users.Remove(toRemove);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<TaskItem> AddIssueAsync(int projectId, TaskItem issue, CancellationToken ct = default)
        {
            var exists = await _db.Projects.AnyAsync(p => p.Id == projectId, ct);
            if (!exists) throw new KeyNotFoundException("Project not found.");

            issue.ProjectId = projectId;
            _db.Issues.Add(issue);
            await _db.SaveChangesAsync(ct);
            return issue;
        }

        public async Task<bool> RemoveIssueAsync(int issueId, CancellationToken ct = default)
        {
            var existing = await _db.Issues.FindAsync([issueId], ct);
            if (existing is null) return false;
            _db.Issues.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<Project?> GetProjectWithUsersAsync(int projectId, CancellationToken ct = default)
        {
            return await _db.Projects
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);
        }
    }
}
