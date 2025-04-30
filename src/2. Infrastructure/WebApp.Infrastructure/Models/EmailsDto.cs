namespace WebApp.Infrastructure.Models
{
    public class EmailsDto
    {
        public string BodyContent { get; set; }
        public List<string> ToRecepients { get; set; } 
        public List<string> CCRecepients { get; set; }
    }
}
