# CodeWith-Kamervragen

**CodeWith-Kamervragen** is a POC to see how RAG can be used on "Tweede Kamer vragen" ([https://gegevensmagazijn.tweedekamer.nl/](https://gegevensmagazijn.tweedekamer.nl/)) to see if questions have been already asked.
The solution makes use of the following Azure infrastructure components

- **Azure AI Search:** for vector search capabilities
- **Azure OpenAI:** for chatcompletion and embeddings
- **Azure CosmosDb:** storing the conversations
- **Azure Blob Storage:** hosting the documents

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Setup and Installation](#setup-and-installation)
- [Azure AD Configuration](#azure-ad-configuration)
- [Deployment](#deployment)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Secure Authentication:** Utilizes Azure AD for managing user identities and access.
- **Interactive Chat Interface:** Enables users to participate in Q&A sessions with real-time responses.
- **Contextual Data Points:** Displays relevant data points within chat messages for enhanced understanding.
- **Role Assignments:** Assigns specific Azure roles to managed identities for resource access.
- **Service Principal Lock Configuration:** Ensures security by locking service principals with defined configurations.

## Architecture

The solution comprises two main components:

1. **Backend Application**
   - **Purpose:** Handles authentication, authorization, and business logic.
   - **Technologies:** Aspire, C#, WebApi, Spectre

2. **Frontend Application**
   - **Purpose:** Provides a user-friendly interface for interacting with the chat system.
   - **Technologies:** React, TypeScript, Fluent UI for styling

![Architecture Diagram](docs/architecture.png)

## Prerequisites

Before setting up the project, ensure you have the following tools and services:

- **Azure Subscription:** Required for Azure AD and resource management.
- **Node.js (v14 or later):** Backend and frontend dependencies.
- **npm or Yarn:** Package manager for JavaScript/TypeScript.
- **PowerShell (v7 or later):** For executing deployment scripts.
- **Microsoft Graph PowerShell Module:** For managing Azure AD via scripts.
- **Git:** Version control system for cloning the repository.
- **Integrated Development Environment (IDE):** Visual Studio Code is recommended.

## Setup and Installation

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/codewith-kamervragen.git
cd codewith-kamervragen
```

### 2. Setup Backend

```bash
cd backend
# Install backend dependencies if applicable
```

### 3. Setup Frontend

```bash
cd frontend
npm install
# or
yarn install
```

## Entra Configuration

To make auth work, we need to have App Registrations in Entra to make sure we can a) logon and b) authorize the user when accessin the application. To configure Entra for this project, you need to create app registrations for both the backend and frontend applications. You can use the provided PowerShell scripts to automate this process.

### 1. Create Backend App Registration

Run the following script to create the backend app registration:

```powershell
cd deploy
.\create-backendAppReg.ps1 -DisplayName "BackendApp" -TenantId "<your-tenant-id>"
```

### 2. Create Backend App Registration

```powershell
cd deploy
.\create-frontendAppReg.ps1 -DisplayName "FrontendApp" -backendAppId "<your-backend-app-id>" -backendScopeId "<your-backend-scope-id>" -backendUrl "<your-backend-url>" -TenantId "<your-tenant-id>"
```

Replace the placeholders (<your-tenant-id>, <your-backend-app-id>, <your-backend-scope-id>, <your-backend-url>) with the appropriate values for your Azure AD setup.

## Deployment

// ...existing code...

## Usage

// ...existing code...

## Contributing

// ...existing code...

## License

// ...existing code...
