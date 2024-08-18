namespace API.Data.DbModel
{
    public class LeadsRetrievalLog
    {
        public int Id { get; set; }
        public DateTime? LastRunTime { get; set; }
        public string Status { get; set; }
    }
}
