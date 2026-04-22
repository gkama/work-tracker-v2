using WorkTracker.Common.Models;
using WorkTracker.Common.Services;

namespace WorkTracker.Common.Extensions
{
    public static class DbTestMigrationExtensions
    {
        public static async Task RunTestMigrationAsync(this WorkTrackerDbContext dbContext, TestMigrationContext? testMigrationContext = null)
        {
            dbContext.Database.EnsureCreated();

            testMigrationContext ??= new TestMigrationContext();

            await dbContext.AddTestUsersAsync(testMigrationContext);
            await dbContext.AddTestOrganizationsAsync(testMigrationContext);
            await dbContext.AddTestProjectsAsync(testMigrationContext);
            await dbContext.AddTestWorkItemsAsync(testMigrationContext);
        }

        private static async Task<WorkTrackerDbContext> AddTestUsersAsync(this WorkTrackerDbContext dbContext, TestMigrationContext testMigrationContext)
        {
            var users = new List<User>
            {
                new()
                {
                    FirstName = "admin",
                    LastName = "admin",
                    Username = "admin",
                    Password = EncryptionService.Hash("admin"),
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    UpdatedAt = DateTime.UtcNow.AddDays(-60),
                },
                new()
                {
                    FirstName = "user",
                    LastName = "user",
                    Username = "user",
                    Password = EncryptionService.Hash("user"),
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    UpdatedAt = DateTime.UtcNow.AddDays(-60),
                }
            };

            foreach (var user in users)
            {
                await dbContext.Users.AddAsync(user);
            }

            await dbContext.SaveChangesAsync();

            testMigrationContext.Users.AddRange(users);

            return dbContext;
        }

        private static async Task<WorkTrackerDbContext> AddTestOrganizationsAsync(this WorkTrackerDbContext dbContext, TestMigrationContext testMigrationContext)
        {
            var orgranizations = new List<Organization>
            {
                new()
                {
                    Name = "Kress Designs",
                    Description = "Kress Designs Inc",
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    UpdatedAt = DateTime.UtcNow.AddDays(-60),
                },
                new()
                {
                    Name = "GK Technologies",
                    Description = "GK Technologies, LLC",
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    UpdatedAt = DateTime.UtcNow.AddDays(-60),
                }
            };

            foreach (var organization in orgranizations)
            {
                await dbContext.Organizations.AddAsync(organization);
            }

            await dbContext.SaveChangesAsync();

            testMigrationContext.Organizations.AddRange(orgranizations);

            return dbContext;
        }


        private static async Task<WorkTrackerDbContext> AddTestProjectsAsync(this WorkTrackerDbContext dbContext, TestMigrationContext testMigrationContext)
        {
            var organizationKd = testMigrationContext.Organizations.First(o => o.Name == "Kress Designs");
            var organizationGk = testMigrationContext.Organizations.First(o => o.Name == "GK Technologies");

            var projects = new List<Project>
            {
                new()
                {
                    Name = "Technologee",
                    Description = "Technologee",
                    OrganizationId = organizationKd.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-50),
                    UpdatedAt = DateTime.UtcNow.AddDays(-50),
                    Organization = organizationKd
                },
                new()
                {
                    Name = "Work Tracker",
                    Description = "Work Tracker",
                    OrganizationId = organizationGk.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15),
                    Organization = organizationGk
                }
            };

            foreach (var project in projects)
            {
                await dbContext.Projects.AddAsync(project);
            }

            await dbContext.SaveChangesAsync();

            testMigrationContext.Projects.AddRange(projects);

            return dbContext;
        }

        private static async Task<WorkTrackerDbContext> AddTestWorkItemsAsync(this WorkTrackerDbContext dbContext, TestMigrationContext testMigrationContext)
        {
            var projectTechnologee = testMigrationContext.Projects.First(o => o.Name == "Technologee");

            var workItems = new List<WorkItem>
            {
                new()
                {
                    Title = "TEC-571",
                    Description = "Feature",
                    ProjectId = projectTechnologee.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7),
                    WorkItemHours =
                    [
                        new()
                        {
                            Description = "Development",
                            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-6)),
                            Hours = 8,
                            CreatedAt = DateTime.UtcNow.AddHours(-6),
                            UpdatedAt = DateTime.UtcNow.AddHours(-6),
                        },
                        new()
                        {
                            Description = "Development",
                            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
                            Hours = 8,
                            CreatedAt = DateTime.UtcNow.AddHours(-5),
                            UpdatedAt = DateTime.UtcNow.AddHours(-5),
                        }
                    ],
                    Project = projectTechnologee
                },
                new()
                {
                    Title = "TEC-572",
                    Description = "Feature",
                    ProjectId = projectTechnologee.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4),
                    WorkItemHours =
                    [
                        new()
                        {
                            Description = "Development",
                            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-4)),
                            Hours = 10,
                            CreatedAt = DateTime.UtcNow.AddHours(-4),
                            UpdatedAt = DateTime.UtcNow.AddHours(-4),
                        }
                    ],
                    Project = projectTechnologee
                }
            };

            foreach (var workItem in workItems)
            {
                await dbContext.WorkItems.AddAsync(workItem);
            }

            await dbContext.SaveChangesAsync();

            testMigrationContext.WorkItems.AddRange(workItems);

            return dbContext;
        }
    }

    public class TestMigrationContext
    {
        public List<User> Users { get; set; } = [];
        public List<Organization> Organizations { get; set; } = [];
        public List<Project> Projects { get; set; } = [];
        public List<WorkItem> WorkItems { get; set; } = [];
    }
}