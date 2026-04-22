namespace WorkTracker.Common.Dtos
{
    public record UserDto : BaseDto
    {
        public int Id { get; init; }
        public required string FirstName { get; init; }
        public string? MiddleName { get; init; }
        public required string LastName { get; init; }
        public string? Email { get; init; }
        public required string Username { get; init; }

        public IReadOnlyCollection<OrganizationDto> Organizations { get; init; } = [];
    }
}
