namespace WebApp.Infrastructure.Helpers
{
    public class ConfigConstants
    {
        public enum ConfigIdsName
        {
            AzureTenantId,
            AzureClientId,
            AzureClientSecret,
            SharePointSiteId,
            SharePointSiteURL,
            ComplianceLibraryName,
            ComplianceLibraryId,
            EmailListId,
            MessagesListId
        }

        public static HashSet<string> StopWords = new HashSet<string>
        {
            "i", "me", "my", "myself", "we", "our", "ours", "ourselves",
            "you", "your", "yours", "yourself", "yourselves",
            "he", "him", "his", "himself", "she", "her", "hers", "herself",
            "it", "its", "itself", "they", "them", "their", "theirs", "themselves",
            "what", "which", "who", "whom", "this", "that", "these", "those",
            "am", "is", "are", "was", "were", "be", "been", "being",
            "have", "has", "had", "having", "do", "does", "did", "doing",
            "a", "an", "the", "and", "but", "if", "or", "because", "as",
            "until", "while", "of", "at", "by", "for", "with", "about", "against",
            "between", "into", "through", "during", "before", "after",
            "above", "below", "to", "from", "up", "down", "in", "out", "on",
            "off", "over", "under", "again", "further", "then", "once",
            "here", "there", "when", "where", "why", "how", "all", "any",
            "both", "each", "few", "more", "most", "other", "some", "such",
            "no", "nor", "not", "only", "own", "same", "so", "than", "too", "very",
            "can", "will", "just", "don", "should", "now"
        };

        public static string AIPrompt = @"
                🛠️ Strict JSON-Format Prompt (with Risk Level)
                Analyze the following {0} content for any compliance or security breaches by searching my company’s uploaded documents in Azure AI Search.

                For each identified breach, return the response strictly in JSON array format as described below.
                Do not add any extra text or explanation outside of the JSON.

                Each breach must be a JSON object containing these fields:

                - BreachNumber (integer starting from 1)
                - Source (""{0}"")
                - DocumentTitle (string)
                - Section (string, optional, empty string """" if not available)
                - PolicySentence (string, exact sentence or line from the document)
                - ViolationExplanation (string, explaining how the action violated the policy)
                - DetectedRiskLevel (""High"", ""Medium"", or ""Low"")

                If no breaches are found, return an empty array like this:
                []

                {0} Content to Analyze:
                ""{1}""
                ";

        public static string agentInstructions = "You help with compliance and security related document queries.";
    }
}
