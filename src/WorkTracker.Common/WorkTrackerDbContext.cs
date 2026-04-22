using Microsoft.EntityFrameworkCore;
using WorkTracker.Common.Models;

namespace WorkTracker.Common
{
    public class WorkTrackerDbContext(DbContextOptions<WorkTrackerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<UserOrganization> UserOrganizations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<WorkItemHours> WorkItemHours { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>()
                .ToTable("user");

            modelBuilder.Entity<User>()
                .HasIndex(o => o.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.MiddleName)
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(500);

            // Organization
            modelBuilder.Entity<Organization>()
                .ToTable("organization");

            // UserOrganization
            modelBuilder.Entity<UserOrganization>()
                .ToTable("user_organization")
                .HasKey(uo => new { uo.UserId, uo.OrganizationId });

            modelBuilder.Entity<UserOrganization>()
                .HasOne(uo => uo.User)
                .WithMany(u => u.Organizations)
                .HasForeignKey(uo => uo.UserId);

            modelBuilder.Entity<UserOrganization>()
                .HasOne(uo => uo.Organization)
                .WithMany(o => o.UserOrganizations)
                .HasForeignKey(uo => uo.OrganizationId);

            // Project
            modelBuilder.Entity<Project>()
                .ToTable("project")
                .HasOne(p => p.Organization)
                .WithMany(o => o.Projects)
                .HasForeignKey(p => p.OrganizationId);

            modelBuilder.Entity<Project>()
                .HasIndex(x => x.Name)
                .IsUnique();

            // WorkItem
            modelBuilder.Entity<WorkItem>()
                .ToTable("work_item")
                .HasOne(wi => wi.Project)
                .WithMany(p => p.WorkItems)
                .HasForeignKey(wi => wi.ProjectId);

            // WorkItemHours
            modelBuilder.Entity<WorkItemHours>()
                .ToTable("work_item_hours");
        }
    }
}
