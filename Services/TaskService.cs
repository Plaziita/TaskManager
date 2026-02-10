
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;           
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TaskService
    {
        private readonly AppDbContext _db;

        public TaskService(AppDbContext db)
        {
            _db = db;
        }

        private DbSet<TaskItem> Tasks => _db.Set<TaskItem>();

       
        public async Task<TaskItem> CreateAsync(int projectId, TaskItem item, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(item.Title))
                throw new ArgumentException("Title is required.", nameof(item.Title));

            var exists = await _db.Projects.AnyAsync(p => p.Id == projectId, ct);
            if (!exists)
                throw new KeyNotFoundException("Project not found.");

            item.ProjectId = projectId;
            item.CreatedAt = DateTime.UtcNow;

            _db.Set<TaskItem>().Add(item);
            await _db.SaveChangesAsync(ct);
            return item;
        }


        public async Task<TaskItem?> GetByIdAsync(int id, bool includeProject = false, CancellationToken ct = default)
        {
            IQueryable<TaskItem> q = Tasks.AsQueryable();
            if (includeProject) q = q.Include(t => t.Project);
            return await q.FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<IReadOnlyList<TaskItem>> GetAllForProjectAsync(int projectId, string? status = null, CancellationToken ct = default)
        {
            var q = Tasks.Where(t => t.ProjectId == projectId);
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(t => t.Status == status);

            return await q.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<TaskItem>> GetAllTaskAsync(
            string userId, CancellationToken ct = default)
        {
            return await _db.Issues
                .AsNoTracking()
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);
        }


        public async Task<(IReadOnlyList<TaskItem> Items, int Total)> GetPagedForProjectAsync(
            int projectId, int page, int pageSize, string? status = null, CancellationToken ct = default)
        {
            var q = Tasks.Where(t => t.ProjectId == projectId);
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(t => t.Status == status);

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(t => t.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(ct);

            return (items, total);
        }

        public async Task<TaskItem> UpdateAsync(TaskItem item, CancellationToken ct = default)
        {
            var existing = await Tasks.FirstOrDefaultAsync(t => t.Id == item.Id, ct)
                           ?? throw new KeyNotFoundException("Task not found.");

            existing.Title = item.Title;
            existing.Description = item.Description;
            existing.Status = item.Status;
            existing.Priority = item.Priority;
            existing.DueDate = item.DueDate;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> ChangeStatusAsync(int id, string status, CancellationToken ct = default)
        {
            var existing = await Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (existing is null) return false;

            existing.Status = status;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await Tasks.FindAsync(new object?[] { id }, ct);
            if (existing is null) return false;

            Tasks.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        
        public async Task<bool> AssignUserAsync(int taskId, string? userId, CancellationToken ct = default)
        {
            var task = await Tasks.FirstOrDefaultAsync(t => t.Id == taskId, ct);
            if (task == null) return false;

            task.AssignedUserId = userId;
            await _db.SaveChangesAsync(ct);
            return true;
        }
        public async Task<IReadOnlyList<TaskItem>> getTaskByRecentDateAsync(string userId, CancellationToken ct = default)
        {
            return await _db.Issues
                .AsNoTracking()
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(3)
                .ToListAsync(ct);
        }

        public async Task<int> getCompletedTaskAsync(string userId, CancellationToken ct = default)
        {
            return await _db.Issues
                .AsNoTracking()
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId)
                .Where(t => t.Status == "Done")
                .CountAsync(ct);
        }

        public async Task<int> getInProgressTaskAsync(string userId, CancellationToken ct = default)
        {
            return await _db.Issues
                .AsNoTracking()
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId)
                .Where(t => t.Status == "In Progress" || t.Status == "Open")
                .CountAsync(ct);
        }

        public async Task<int> getOverdueTask(string userId, CancellationToken ct = default)
        {
            
            var todayUtc = DateTime.UtcNow.Date;

            return await _db.Issues
                .AsNoTracking()
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId)
                .Where(t => t.DueDate < todayUtc)
                .CountAsync(ct);
        }


        public Task<int> CountByStatusAsync(int projectId, string status, CancellationToken ct = default) =>
            Tasks.CountAsync(t => t.ProjectId == projectId && t.Status == status, ct);
    }

}
