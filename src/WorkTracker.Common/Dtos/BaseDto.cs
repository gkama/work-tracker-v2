namespace WorkTracker.Common.Dtos
{
    public abstract record BaseDto
    {
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
