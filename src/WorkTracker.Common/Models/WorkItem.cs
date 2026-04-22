namespace WorkTracker.Common.Models
{
    public class WorkItem : BaseModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int ProjectId { get; set; }

        public required Project Project { get; set; }
        public List<WorkItemHours> WorkItemHours { get; set; } = [];
    }
}
