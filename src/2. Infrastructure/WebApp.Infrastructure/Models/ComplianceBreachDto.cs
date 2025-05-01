namespace WebApp.Infrastructure.Models
{
    public class ComplianceBreachDto
    {
        public int BreachNumber { get; set; }
        public string? Source { get; set; }
        public string? DocumentTitle { get; set; }
        public string? Section { get; set; }
        public string? PolicySentence { get; set; }
        public string? ViolationExplanation { get; set; }
        public string? DetectedRiskLevel { get; set; }
        public string? MainKeyword { get; set; }
    }

}
