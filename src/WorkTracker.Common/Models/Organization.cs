namespace WorkTracker.Common.Models
{
    public class Organization : BaseModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        public ICollection<Project> Projects { get; set; } = [];
        public ICollection<UserOrganization> UserOrganizations { get; set; } = [];
    }
}
