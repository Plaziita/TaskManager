
using Microsoft.AspNetCore.Identity;

namespace TaskManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    }
}
