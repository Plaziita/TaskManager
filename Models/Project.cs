using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        // Metadatos
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
