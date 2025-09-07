# Developer Guide

## Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- MySQL Server access
- Git

## Local Development Setup

### 1. Clone and Setup
```bash
git clone <repository-url>
cd OnlineEventTicketing
dotnet restore OnlineEventTicketing.sln
```

### 2. Database Configuration
Create `appsettings.Development.json` in the project root:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=online_event_ticketing;Uid=root;Pwd=your-password;"
  }
}
```

### 3. Update Database
**Command Line:**
```bash
cd OnlineEventTicketing
dotnet ef database update
```

**Visual Studio:**
- Tools → NuGet Package Manager → Package Manager Console
- Run: `Update-Database`

### 4. Run Application
**Command Line:**
```bash
dotnet run --project OnlineEventTicketing.csproj
```

**Visual Studio:**
- Press F5 or click "Start Debugging"

Application runs at `https://localhost:5001`

## Development Commands

### Database Migrations
**Command Line:**
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

**Visual Studio Package Manager Console:**
```powershell
# Add new migration
Add-Migration MigrationName

# Update database
Update-Database
```

### Build & Test
**Command Line:**
```bash
# Build
dotnet build OnlineEventTicketing.sln

# Test
dotnet test OnlineEventTicketing.sln
```

**Visual Studio:**
- Build: Ctrl+Shift+B or Build → Build Solution
- Test: Test → Run All Tests

## Git Workflow
1. Create feature branch: `git checkout -b feature/your-feature`
2. Make changes and commit
3. Push and create pull request

## Configuration Notes
- `appsettings.Development.json` is gitignored for security
- Production uses Azure App Settings
- Connection string format: `Server=host;Database=db;Uid=user;Pwd=pass;`