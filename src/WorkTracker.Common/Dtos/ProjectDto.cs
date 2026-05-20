namespace WorkTracker.Common.Dtos
{
    public record ProjectDto : BaseDto
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int OrganizationId { get; init; }

        public IReadOnlyCollection<WorkItemDto> WorkItems { get; init; } = [];
    }
}
