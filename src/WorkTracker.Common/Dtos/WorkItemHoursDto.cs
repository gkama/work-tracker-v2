namespace WorkTracker.Common.Dtos
{
    public record WorkItemHoursDto : BaseDto
    {
        public int Id { get; init; }
        public int WorkItemId { get; init; }
        public string? Description { get; init; }
        public DateOnly Date { get; init; }
        public decimal Hours { get; init; }
    }
}
