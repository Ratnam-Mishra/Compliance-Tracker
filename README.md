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

## ⚙️ Project Structure
#### WebApp.Infrastructure

The infrastructure layer contains the core components shared across other parts of the application.

- **WebApp.Infrastructure.dll**: Core library with infrastructure components.
- **bin/Debug/net8.0**: Compiled output for the WebApp.Infrastructure project.
  - **Azure.AI.OpenAI.dll**: For integrating with Azure's OpenAI services.
  - **Microsoft.Graph.dll**: For interacting with the Microsoft Graph API.
  - **WebApp.Infrastructure.pdb**: Debug symbols for WebApp.Infrastructure.
  - **WebApp.Infrastructure.exe**: Executable for running the infrastructure layer.

#### Schedulers

This directory contains the scheduler component responsible for running compliance analysis.

- **appsettings.json**: Configuration file for the scheduler component.
- **Program.cs**: Main entry point for the scheduler service.
- **Schedulers.csproj**: Project file for the scheduler component.
- **UserComplianceAnalyzer.cs**: Class responsible for analyzing user compliance.

#### Controllers

- **ComplianceController.cs**: Controller for handling compliance-related API requests.

#### Infrastructure

The **Infrastructure** directory contains classes that assist in compliance processing and external integrations.

- **Agents/ComplianceAgent.cs**: Agent responsible for managing compliance checks.
- **Helpers**: Utility classes for various tasks within the infrastructure.
  - **ConfigConstants.cs**: Configuration constants used throughout the project.
  - **Configuration.cs**: Configuration handling logic.
  - **CustomHeadersPolicy.cs**: Custom headers policy for API requests.
  - **EmbeddingService.cs**: Service for embedding and analyzing content.
  - **FilesTextExtracter.cs**: Helper for extracting text from files.
  - **GraphSharePointHelper.cs**: Helper for working with Microsoft Graph and SharePoint.
  - **SearchService.cs**: Service for performing search operations related to compliance.

#### Models

The **Models** directory contains data transfer objects (DTOs) used for communication between services.

- **ComplianceBreachDto.cs**: DTO for representing compliance breaches.
- **DocumentDto.cs**: DTO for documents.
- **EmailsDto.cs**: DTO for email data.
- **UploadFileDto.cs**: DTO for file uploads.
- **UsersDto.cs**: DTO for user data.
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
