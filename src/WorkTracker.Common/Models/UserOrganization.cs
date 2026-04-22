namespace WorkTracker.Common.Models
{
    public class UserOrganization : BaseModel
    {
        public int UserId { get; set; }
        public int OrganizationId { get; set; }

        public required User User { get; set; }
        public required Organization Organization { get; set; }
    }
}
