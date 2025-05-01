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
                🛠️ Strict JSON-Format Prompt (with Risk Level & Keyword)
                Analyze the following {0} content for any compliance or security breaches by comparing it with my organization’s uploaded documents in Azure AI Search.

                Return your response strictly as a **JSON array**. Do not include any explanations or additional text outside the array.

                Each item in the array must be a JSON object with the following fields:

                - BreachNumber (integer starting from 1)
                - Source (""{0}"" — either ""Teams Chat"" or ""Email"")
                - DocumentTitle (string)
                - Section (string, optional, set to """" if not applicable)
                - PolicySentence (string, the exact matching sentence or excerpt from the document)
                - ViolationExplanation (string, explaining how the policy was violated)
                - DetectedRiskLevel (""High"", ""Medium"", or ""Low"")
                - MainKeyword (string, the primary term or phrase that triggered the match — e.g., ""password"", ""confidential"", etc.)

                If no violations are detected, return exactly:
                []

                {0} Content to Analyze:
                ""{1}""
                ";

        public static string agentInstructions = "You help with compliance and security related document queries.";
    }
}
