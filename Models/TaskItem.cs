
namespace TaskManager.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Relación con Project
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        //Una persona asignada
        public string? AssignedUserId { get; set; }
        public ApplicationUser? AssignedUser { get; set; }

        // Campos de estado
        public string Status { get; set; } = "Open";
        public int Priority { get; set; } = 3;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
    }
}
