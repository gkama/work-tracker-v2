namespace WorkTracker.Common.Models
{
    public class WorkItemHours : BaseModel
    {
        public int Id { get; set; }
        public int WorkItemId { get; set; }
        public string? Description { get; set; }
        public DateOnly Date { get; set; }
        public decimal Hours { get; set; }
    }
}
