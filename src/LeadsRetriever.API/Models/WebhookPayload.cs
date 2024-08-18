namespace API.Models
{
    public class WebhookPayload
    {
        public string Action { get; set; }
        public LeadDto Lead { get; set; }
    }
}
