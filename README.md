# OPM Egnyte App

A .NET 8 Razor Pages application for managing Egnyte file system permissions and folder access.

## Product Requirements Document

### Purpose
Enable efficient management of Egnyte folder permissions through a user-friendly web interface, focusing on group-based access control and bulk operations.

### Target Users
- System Administrators
- Project Managers
- IT Support Staff
- Permission Administrators

### Technical Requirements

1. **Security**
   - OAuth 2.0 authentication with Egnyte
   - Secure storage of credentials and tokens
   - HTTPS encryption
   - CSRF protection

2. **Performance**
   - Fast folder navigation
   - Efficient bulk operations
   - Responsive UI

3. **Reliability**
   - Error handling and recovery
   - Validation of permissions
   - Atomic operations

## Minimum Viable Product (MVP)

### Phase 1: Core Authentication - ? Completed
1. **User Authentication**
   - [x] OAuth 2.0 integration
   - [x] Login page
   - [x] Session management
   - [x] Token handling

2. **Security Foundation**
   - [x] HTTPS enforcement
   - [x] Secure token storage
   - [x] User secrets management

### Phase 2: Basic File System - ? Completed
1. **Folder Navigation**
   - [x] List folders and files
   - [x] Folder metadata display
   - [x] Breadcrumb navigation
   - [x] Path validation

2. **UI Components**
   - [x] Folder tree view
   - [x] File/folder icons
   - [x] Dark/light theme support

### Phase 3: Permission Management - ? Completed
1. **View Permissions**
   - [x] Display current permissions
   - [x] Show inheritance status
   - [x] Group permission display
   - [x] Permission level indicators

2. **Modify Permissions**
   - [x] Set group permissions
   - [x] Validate permission changes
   - [x] Error handling
   - [x] Success confirmation

### Phase 4: Bulk Operations - ? Completed
1. **Bulk Updates**
   - [x] Select multiple folders
   - [x] Batch permission changes
   - [x] Progress tracking
   - [x] Error reporting

2. **Operation Management**
   - [x] Validation checks
   - [x] Transaction handling
   - [x] Status updates
   - [x] Rollback support

### Future Phases

#### Phase 5: Advanced Operations
1. **File Management**
   - [ ] Upload/Download
   - [ ] Move/Copy
   - [ ] Delete operations
   - [ ] File versioning

2. **Permission Templates**
   - [ ] Create templates
   - [ ] Apply to multiple folders
   - [ ] Template management
   - [ ] Default templates

#### Phase 6: Monitoring & Reporting
1. **Audit System**
   - [ ] Permission change logs
   - [ ] User activity tracking
   - [ ] Report generation
   - [ ] Export capabilities

2. **Analytics**
   - [ ] Usage statistics
   - [ ] Permission analytics
   - [ ] Performance metrics
   - [ ] Health monitoring

## Key Features

- **Authentication**
  - Secure OAuth 2.0 integration with Egnyte
  - Multiple authentication flows (Resource Owner Password & Authorization Code)
  - Session management and token handling

- **File System Management**
  - Browse files and folders with a modern UI
  - Tree-style folder navigation with breadcrumbs
  - File metadata display (size, type, etc.)
  - AJAX-powered dynamic folder loading

- **Permission Management**
  - Individual and bulk permission updates
  - Group-based access control
  - Permission inheritance visualization
  - Granular permission levels (None to Owner)

- **Security & Integration**
  - Secure credential storage using ASP.NET Core User Secrets
  - HTTPS enforcement
  - Comprehensive error handling
  - Production-ready configuration options

## Prerequisites

- .NET 8 SDK
- Egnyte API credentials
- HTTPS certificate (development or production)

## Quick Start

1. Clone the repository
2. Configure secrets:
   ```sh
   dotnet user-secrets set "Egnyte:ClientId" "<your-client-id>"
   dotnet user-secrets set "Egnyte:ClientSecret" "<your-client-secret>"
   ```
3. Update `appsettings.json` with your Egnyte domain and endpoints
4. Run the application:
   ```sh
   dotnet run
   ```

## Configuration

### Development
Use ASP.NET Core User Secrets for local development:
```sh
dotnet user-secrets init
dotnet user-secrets set "Egnyte:ClientId" "<your-client-id>"
dotnet user-secrets set "Egnyte:ClientSecret" "<your-client-secret>"
```

### Production
Set environment variables:
- `Egnyte__ClientId`
- `Egnyte__ClientSecret`
- `Egnyte__Domain`

## Architecture

### Core Components

1. **Authentication (Auth/)**
   - EgnyteAuthService: OAuth flow management
   - EgnyteAuthOptions: Auth configuration
   - Login/Callback handlers

2. **File System (Services/)**
   - EgnyteFolderService: File/folder operations
   - Permission management
   - API integration

3. **User Interface (Pages/)**
   - Folder browsing
   - Permission management
   - Bulk operations
   - Error handling

4. **Models (Models/)**
   - API response types
   - Permission models
   - File system DTOs

## Security Features

- OAuth 2.0 compliant authentication
- Secure secret management
- HTTPS requirement
- Input validation
- XSS protection
- CSRF protection

## Error Handling

- Comprehensive error logging
- User-friendly error messages
- API error translation
- Permission denial handling
- Network error recovery

## MVP Features

### 1. Authentication
- [x] Resource Owner Password Flow
- [x] Secure token management
- [x] Session handling

### 2. File System
- [x] Folder browsing
- [x] File listing
- [x] Path navigation

### 3. Permissions
- [x] View permissions
- [x] Set group permissions
- [x] Bulk updates

## Future Enhancements

1. File Operations
   - Upload/Download
   - Move/Copy
   - Delete operations

2. Advanced Permissions
   - User-level permissions
   - Permission templates
   - Batch operations

3. Monitoring
   - Activity logging
   - Audit trails
   - Usage statistics

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support, please contact the project maintainers or raise an issue in the repository.
