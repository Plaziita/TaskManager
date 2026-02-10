
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TaskItem> Issues => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Project>()
                .HasMany(p => p.Users)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectUser",
                    j => j.HasOne<ApplicationUser>()
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasPrincipalKey(u => u.Id)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<Project>()
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .HasPrincipalKey(p => p.Id)
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("ProjectId", "UserId");
                        j.ToTable("ProjectUsers");
                    }
                );

            builder.Entity<Project>()
                .HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull); 

            builder.Entity<Project>(e =>
            {
                e.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();
            });
        }

    }
}
