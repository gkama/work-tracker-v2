namespace WorkTracker.Common.Dtos
{
    public record OrganizationDto : BaseDto
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }

        public IReadOnlyCollection<ProjectDto> Projects { get; init; } = [];
    }
}
