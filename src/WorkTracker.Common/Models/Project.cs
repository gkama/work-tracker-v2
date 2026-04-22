namespace WorkTracker.Common.Models
{
    public class Project : BaseModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int OrganizationId { get; set; }

        public required Organization Organization { get; set; }
        public List<WorkItem> WorkItems { get; set; } = [];
    }
}
