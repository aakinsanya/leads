namespace API.Data.DbModel
{
    public class Lead
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? SourceCreatedDate { get; set; }
        public string SourceId { get; set; }
    }
}
