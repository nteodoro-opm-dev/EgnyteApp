# EgnyteApp

A .NET 8 Razor Pages application for authenticating with Egnyte and browsing files/folders using the Egnyte API.

## Features

- **Egnyte OAuth 2.0 Authentication** (Authorization Code and Resource Owner Password flows)
- **Secure login** with Egnyte credentials
- **Browse files and folders** with a tree-style, expandable UI
- **File size formatting** (B, KB, MB, GB, TB)
- **File/folder icons** (folder, file, zip, etc.)
- **AJAX-powered subfolder expansion**
- **Folder Permission Management**

## Standard Folder Structure and Permissions

| Folder Path | Permission Level |
|-------------|-----------------|
| 1 Master List | No Access |
| 2 Permits, Contract Documents, Letters | ------- |
| ?? \Permits, Contract Documents, Letters\Permits | View |
| ?? \Permits, Contract Documents, Letters\Contract | No Access |
| ?? \Permits, Contract Documents, Letters\Letters | No Access |
| 3 OCIP | No Access |
| 4 Drawings, Specs | View |
| 5 Suppliers Quotes & P.Os | Full |
| ?? \Suppliers Quotes & P.Os\Packing Slips | View |
| 6 Fixtures | View |
| 7 Equipment | View |
| 8 Project Schedules | View |
| 9 Progress | No Access |
| 10 Change Orders | View |
| 11 Reports | View |
| 12 RFIs, ASIs | View |
| 13 Meeting Minutes | Editor |
| 14 Transmittals - Job Information Sheets | No Access |
| 15 Submittals - Shop Drawings | -------- |
| ?? \Shop Drawings\Shop Drawings (Received from Vendor) | No Access |
| ?? \Shop Drawings\Shop Drawings (Reviewed) | View |
| ?? \Shop Drawings\Shop Drawings (Submitted) | No Access |
| 16 O+M Manuals | View |
| 17 Controls | Editor |
| 18 QA-QC | Full |
| 19 Safety Manual | View |
| 20 Code Book | View |

## Prerequisites

- .NET 8 SDK
- Egnyte API credentials (Client ID, Client Secret, etc.)

## Setup

1. Clone the repository.
2. Update `appsettings.json` with your Egnyte API credentials and endpoints.
3. Run the project:
   ```sh
   dotnet run
   ```
4. Open the app in your browser (e.g., https://localhost:5001).

## Secret Management

**Never commit real secrets to version control.**

- `appsettings.json` contains placeholders for `ClientId` and `ClientSecret`.
- The file is listed in `.gitignore` to prevent accidental commits.
- For local development, use [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):
  ```sh
  dotnet user-secrets set "Egnyte:ClientId" "<your-client-id>"
  dotnet user-secrets set "Egnyte:ClientSecret" "<your-client-secret>"
  ```
- In production, set secrets as environment variables:
  - `Egnyte__ClientId`
  - `Egnyte__ClientSecret`

## Project Structure

The application is organized into the following components:

### Authentication (Auth folder)
- `EgnyteAuthService`: Handles OAuth 2.0 authentication flows
- `EgnyteAuthOptions`: Configuration for authentication endpoints

### Folder Management (Services folder)
- `EgnyteFolderService`: Handles folder operations (listing, permissions)

### Models (Models folder)
- Response types for API interactions
- Data transfer objects

### Pages
- Authentication pages (login, logout)
- Folder management pages (list, permissions)

## Features

### Authentication

- **Resource Owner Password Flow:**
  - Direct login with Egnyte credentials
  - Secure token management
- **Authorization Code Flow:**
  - OAuth 2.0 standard flow (optional)

### Folder Operations

- Browse folders and files
- View and set permissions
- Support for special characters (including !)
- Error handling and validation

## Security Features

- Secure token handling
- Input validation
- Error handling
- HTTPS enforcement
- Cookie protection

## Dependencies

- Built-in ASP.NET Core authentication
- System.Text.Json (built into .NET 8)
- Bootstrap Icons for UI

## Code Organization

- Separated authentication and folder management concerns
- Clean architecture principles
- Proper dependency injection
- Consistent error handling

## Error Handling

All API errors are properly handled and displayed in the UI:
- Authentication errors
- Permission errors (403 Forbidden)
- Invalid input validation
- Server errors

For further customization or support, please refer to the Egnyte API documentation or contact the project maintainer.

## Minimum Viable Product (MVP)

The core functionality of this application can be broken down into these essential features:

### 1. Basic Authentication
- Resource Owner Password Flow (username/password login)
- Secure token management
- Session handling

### 2. Core File Operations
- List folders and files
- Navigate through folder hierarchy
- Basic file information display

### 3. Permission Management
- View folder permissions
- Set group permissions
- Handle basic error cases

### MVP Implementation Steps

1. **Setup Project**
   ```sh
   dotnet new webapp -n EgnyteApp
   cd EgnyteApp
   ```

2. **Core Services**
   - EgnyteAuthService (authentication)
   - EgnyteFolderService (file operations)

3. **Essential Pages**
   - Login
   - List Folder
   - Get/Set Permissions

4. **Basic Security**
   - HTTPS
   - Token management
   - Input validation

## AI Prompt Template

To recreate this project, you can use the following prompt template:

```
I want to create a .NET 8 Razor Pages web application that integrates with the Egnyte API. The application should:

1. Implement OAuth 2.0 authentication with Egnyte using Resource Owner Password flow
2. Allow users to:
   - Browse folders and files
   - View and set folder permissions for groups
   - Navigate through folder hierarchy with breadcrumb navigation
3. Include proper error handling and security measures
4. Use Bootstrap for UI and Bootstrap Icons for file/folder icons

Technical requirements:
- .NET 8 Razor Pages
- Clean architecture with separate services for auth and folder operations
- Secure configuration management
- Proper input validation and error handling
- Session management for auth tokens

Please help me implement this step by step, starting with:
1. Project structure and core services
2. Authentication implementation
3. Folder browsing functionality
4. Permission management
5. UI components and navigation

Include security best practices and proper error handling throughout the implementation.
```

Use this prompt with an AI model that can help with code generation, and follow the implementation steps to recreate this project. Make sure to:
1. Never commit real credentials
2. Use proper security practices
3. Test thoroughly
4. Implement error handling
5. Follow the MVP approach before adding advanced features
