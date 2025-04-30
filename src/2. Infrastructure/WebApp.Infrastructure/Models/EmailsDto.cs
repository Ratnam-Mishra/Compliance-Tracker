namespace WebApp.Infrastructure.Models
{
    public class EmailsDto
    {
        public string BodyContent { get; set; }
        public List<string> ToRecipients { get; set; } 
        public List<string> CcRecipients { get; set; }
    }
}
