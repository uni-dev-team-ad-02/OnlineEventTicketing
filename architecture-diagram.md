# Online Event Ticketing System - Architecture Diagram

## System Architecture Overview

```mermaid
graph TB
    subgraph "External Systems"
        User[👤 User/Customer]
        Admin[👨‍💼 Admin User]
        Browser[🌐 Web Browser]
        CI[🚀 GitHub Actions CI/CD]
        Azure[☁️ Azure Web App]
    end

    subgraph "Presentation Layer"
        subgraph "ASP.NET Core MVC Web Application"
            Controllers[🎮 Controllers]
            Views[🖼️ Razor Views]
            Identity[🔐 ASP.NET Identity UI]
            StaticFiles[📁 Static Files (CSS/JS)]
        end
    end

    subgraph "Business Logic Layer"
        subgraph "ASP.NET Core Services"
            AuthService[🔑 Authentication Service]
            Logger[📝 Logging Service]
            Middleware[⚙️ Middleware Pipeline]
        end
    end

    subgraph "Data Access Layer"
        DbContext[🗄️ ApplicationDbContext<br/>Entity Framework Core]
    end

    subgraph "Database Layer"
        MySQL[🐬 MySQL Database<br/>online_event_ticketing]
        subgraph "Database Tables"
            AspNetUsers[👥 AspNetUsers]
            AspNetRoles[🎭 AspNetRoles]
            AspNetUserRoles[🔗 AspNetUserRoles]
            AspNetUserClaims[📋 AspNetUserClaims]
            AspNetUserLogins[🔑 AspNetUserLogins]
            AspNetUserTokens[🎫 AspNetUserTokens]
            AspNetRoleClaims[📜 AspNetRoleClaims]
        end
    end

    subgraph "Configuration"
        AppSettings[⚙️ appsettings.json<br/>Connection Strings<br/>Logging Config]
        Secrets[🔒 User Secrets<br/>Development Config]
    end

    subgraph "Deployment Pipeline"
        GitHub[📚 GitHub Repository]
        Build[🔨 Build & Test]
        Artifacts[📦 Build Artifacts]
        Deploy[🚀 Azure Deployment]
    end

    %% User Interactions
    User --> Browser
    Admin --> Browser
    Browser --> Controllers
    
    %% MVC Pattern
    Controllers --> Views
    Controllers --> AuthService
    Controllers --> Logger
    Views --> StaticFiles
    
    %% Identity System
    AuthService --> Identity
    Identity --> DbContext
    
    %% Data Flow
    Controllers --> DbContext
    DbContext --> MySQL
    
    %% Database Relations
    MySQL --> AspNetUsers
    MySQL --> AspNetRoles
    MySQL --> AspNetUserRoles
    MySQL --> AspNetUserClaims
    MySQL --> AspNetUserLogins
    MySQL --> AspNetUserTokens
    MySQL --> AspNetRoleClaims
    
    %% Configuration
    Controllers --> AppSettings
    DbContext --> AppSettings
    AuthService --> Secrets
    
    %% CI/CD Pipeline
    GitHub --> CI
    CI --> Build
    Build --> Artifacts
    Artifacts --> Deploy
    Deploy --> Azure
    
    %% Styling
    classDef presentation fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef business fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef data fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef database fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef external fill:#fafafa,stroke:#424242,stroke-width:2px
    classDef config fill:#fff8e1,stroke:#ff6f00,stroke-width:2px
    classDef cicd fill:#f1f8e9,stroke:#33691e,stroke-width:2px
    
    class Controllers,Views,Identity,StaticFiles presentation
    class AuthService,Logger,Middleware business
    class DbContext data
    class MySQL,AspNetUsers,AspNetRoles,AspNetUserRoles,AspNetUserClaims,AspNetUserLogins,AspNetUserTokens,AspNetRoleClaims database
    class User,Admin,Browser,Azure external
    class AppSettings,Secrets config
    class GitHub,CI,Build,Artifacts,Deploy cicd
```

## Architecture Components

### 1. **Presentation Layer**
- **ASP.NET Core MVC**: Web framework handling HTTP requests/responses
- **Controllers**: Handle user requests and coordinate responses
  - `HomeController`: Basic application pages (Index, Privacy, Error)
- **Razor Views**: Server-side rendering of HTML pages
  - Home views (Index, Privacy)
  - Shared layout components
  - Identity UI for authentication
- **Static Files**: CSS, JavaScript, and other web assets

### 2. **Business Logic Layer**
- **Authentication Service**: Built-in ASP.NET Core Identity
- **Logging**: Structured logging with configurable levels
- **Middleware Pipeline**: Request processing pipeline including HTTPS redirection, static files, routing, authorization

### 3. **Data Access Layer**
- **Entity Framework Core**: ORM for database operations
- **ApplicationDbContext**: Database context extending IdentityDbContext
- **Connection Management**: MySQL database connectivity

### 4. **Database Layer**
- **MySQL Database**: Primary data storage (`online_event_ticketing`)
- **Identity Tables**: Complete ASP.NET Identity schema
  - Users, Roles, Claims, Logins, Tokens management
- **Migrations**: Database version control and schema updates

### 5. **Configuration Management**
- **appsettings.json**: Application configuration
- **User Secrets**: Development-time secret management
- **Environment-specific**: Development vs Production configurations

### 6. **Deployment Pipeline**
- **GitHub Actions**: Automated CI/CD pipeline
- **Build Process**: .NET 8.0 build and test automation
- **Azure Deployment**: Production deployment to Azure Web Apps
- **Artifact Management**: Build output packaging and deployment

## Technology Stack

- **Framework**: ASP.NET Core 8.0 (MVC)
- **Authentication**: ASP.NET Core Identity
- **Database**: MySQL with Entity Framework Core
- **ORM**: Entity Framework Core with MySQL provider
- **UI**: Razor Pages/Views with Bootstrap
- **Development**: Visual Studio, .NET CLI
- **CI/CD**: GitHub Actions
- **Hosting**: Azure Web Apps
- **Version Control**: Git (GitHub)

## Key Features Identified

1. **User Management**: Complete identity system with registration, login, roles
2. **Web Interface**: MVC-based web application
3. **Database Integration**: MySQL with EF Core migrations
4. **Security**: Authentication, authorization, HTTPS enforcement
5. **Logging**: Configurable application logging
6. **Deployment**: Automated CI/CD with Azure integration
7. **Development Tools**: Comprehensive development setup

## Current State

This appears to be a **foundational setup** for an online event ticketing system with:
- ✅ Basic ASP.NET Core MVC structure
- ✅ Identity system configured
- ✅ Database connectivity established  
- ✅ CI/CD pipeline implemented
- ⏳ **Event management features to be implemented**
- ⏳ **Ticketing functionality to be added**
- ⏳ **Payment integration pending**
- ⏳ **Event catalog and booking system pending**

The architecture provides a solid foundation for building out the core event ticketing functionality.