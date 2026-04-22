namespace WorkTracker.Common.Dtos
{
    public record WorkItemDto : BaseDto
    {
        public int Id { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
        public int ProjectId { get; init; }

        public IReadOnlyCollection<WorkItemHoursDto> WorkItemHours { get; init; } = [];
    }
}
