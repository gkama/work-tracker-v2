using WorkTracker.Common.Dtos;
using WorkTracker.Common.Models;

namespace WorkTracker.Common.Extensions
{
    public static class DtoExtensions
    {
        public static UserDto ToDto(this User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            Email = user.Email,
            Username = user.Username,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Organizations = user.Organizations
                .Select(uo => uo.Organization.ToDto())
                .ToList()
                .AsReadOnly()
        };

        public static OrganizationDto ToDto(this Organization org) => new()
        {
            Id = org.Id,
            Name = org.Name,
            Description = org.Description,
            CreatedAt = org.CreatedAt,
            UpdatedAt = org.UpdatedAt,
            Projects = org.Projects
                .Select(p => p.ToDto())
                .ToList()
                .AsReadOnly()
        };

        public static ProjectDto ToDto(this Project project) => new()
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OrganizationId = project.OrganizationId,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            WorkItems = project.WorkItems
                .Select(w => w.ToDto())
                .ToList()
                .AsReadOnly()
        };

        public static WorkItemDto ToDto(this WorkItem workItem) => new()
        {
            Id = workItem.Id,
            Title = workItem.Title,
            Description = workItem.Description,
            ProjectId = workItem.ProjectId,
            CreatedAt = workItem.CreatedAt,
            UpdatedAt = workItem.UpdatedAt,
            WorkItemHours = workItem.WorkItemHours
                .Select(h => h.ToDto())
                .ToList()
                .AsReadOnly()
        };

        public static WorkItemHoursDto ToDto(this WorkItemHours hours) => new()
        {
            Id = hours.Id,
            WorkItemId = hours.WorkItemId,
            Description = hours.Description,
            Date = hours.Date,
            Hours = hours.Hours,
            CreatedAt = hours.CreatedAt,
            UpdatedAt = hours.UpdatedAt
        };
    }
}
