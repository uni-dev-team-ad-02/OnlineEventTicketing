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

## Email Configuration

The application includes email functionality for sending ticket purchase confirmations. To configure email settings:

### 1. Add Email Configuration
Add the following to your `appsettings.json` or `appsettings.Development.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "your-email@gmail.com",
    "FromName": "Online Event Ticketing",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true
  }
}
```

### 2. Gmail Setup
For Gmail, you'll need to:
1. Enable 2-factor authentication on your Google account
2. Generate an App Password:
   - Go to Google Account settings
   - Security → 2-Step Verification → App passwords
   - Generate a password for "Mail"
   - Use this app password in the configuration

### 3. Other Email Providers
For other email providers, update the SMTP settings accordingly:
- **Outlook/Hotmail**: `smtp-mail.outlook.com`, port 587
- **Yahoo**: `smtp.mail.yahoo.com`, port 587
- **Custom SMTP**: Use your provider's SMTP settings

### 4. Email Features
- Automatic email confirmation when tickets are purchased
- Professional HTML email template with event and ticket details
- QR code included in email for easy access
- Error logging for failed email sends

## Configuration Notes
- `appsettings.Development.json` is gitignored for security
- Production uses Azure App Settings
- Connection string format: `Server=host;Database=db;Uid=user;Pwd=pass;`
- Keep email credentials secure and never commit them to version control