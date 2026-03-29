namespace ConvenientStoreManagement.ViewModels
{
    public class SearchMemberRequest
    {
        public string? PhoneNumber { get; set; }
    }

    public class CreateMemberRequest
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
