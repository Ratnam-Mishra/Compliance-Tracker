# 🛡️ Compliance Agent Bot

The **Compliance Agent Bot** is an intelligent monitoring solution built using Azure AI and Microsoft Graph. It analyzes messages and emails for compliance violations and allows users to upload relevant compliance documents. This solution helps automate compliance tracking across communication platforms in a secure and scalable way.

---

## 📌 Features

- ✅ **Azure AI Agent Integration** – Uses Azure AI Agent SDK to process messages and emails.
- 📬 **Email & Message Scanning** – Automatically scans Microsoft 365 emails and Teams messages.
- 🔍 **Keyword Violation Detection** – Extracts flagged keywords based on predefined rules.
- 📁 **Compliance Document Upload** – Allows users to submit compliance-related documents.
- 📊 **Date-Based Filtering** – Enables filtering of violations by modified date.
- 🔐 **Secured with Microsoft Identity** – Uses `DefaultAzureCredential` for secure Azure authentication.

---

## 🚀 Tech Stack

- **ASP.NET Core** (.NET 6+)
- **Azure AI Agent SDK**
- **Microsoft Graph API**
- **Azure Identity**
- **SharePoint Online** (for storing scanned records and documents)

---

## ⚙️ Configuration

Update `appsettings.json` or Azure configuration with the following settings:

```json

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureTenantId": "<Your-Tenant-ID>",
  "AzureClientId": "<Your-Client-ID>",
  "AzureClientSecret": "<Your-Client-Secret>",
  "AzureSecretId": "<Your-Secret-ID>",

  "SharePointSiteId": "<Your-SharePoint-Site-ID>",
  "ComplianceLibraryId": "<Your-Library-ID>",
  "EmailListId": "<Your-Email-List-ID>",
  "MessagesListId": "<Your-Messages-List-ID>",

  "AzureAI": {
    "Endpoint": "<Your-Azure-AI-Endpoint>",
    "ApiKey": "<Your-Azure-AI-API-Key>",
    "DeploymentName": "<Your-Azure-AI-Deployment-Name>",
    "EmbeddingModelName": "<Your-Embedding-Model-Name>",
    "VectorSearchProfileName": "<Your-Vector-Search-Profile>",
    "VectorSearchHnswConfig": "<Your-HNSW-Config-Name>",
    "ConnectionString": "<Your-AI-Project-Connection-String>",
    "AgentName": "<Your-Agent-Name>"
  },

  "AzureSearch": {
    "IndexName": "<Your-Search-Index-Name>",
    "ApiKey": "<Your-Azure-Search-API-Key>",
    "Endpoint": "<Your-Azure-Search-Endpoint>"
  }
}
