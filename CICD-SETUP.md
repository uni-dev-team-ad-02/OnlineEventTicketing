# CI/CD Setup Guide

## Overview
This repository now includes a GitHub Actions CI/CD pipeline for the TestCICD ASP.NET MVC application.

## Workflow Features
- **Continuous Integration**: Builds and tests on every push/PR
- **Automatic Deployment**: Deploys to production (tag-based)
- **Artifact Management**: Stores build artifacts for deployment

## Environment Setup Required

### 1. GitHub Environments
Create this environment in your GitHub repository settings:

1. Go to Settings → Environments
2. Create `production` environment
3. Configure protection rules as needed

### 2. Deployment Configuration
Update the deployment steps in `.github/workflows/ci-cd.yml` with your specific deployment method:

**For Azure Web App:**
```yaml
- name: Deploy to Azure Web App
  uses: azure/webapps-deploy@v2
  with:
    app-name: your-app-name
    publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
    package: ./publish/
```

**For Docker:**
```yaml
- name: Build and Deploy Docker
  run: |
    docker build -t your-app:latest ./publish/
    docker push your-registry/your-app:latest
```

### 3. Required Secrets
Add these secrets in GitHub Settings → Secrets and variables → Actions:
- `AZURE_CREDENTIALS` - Azure service principal credentials
- `AZURE_APP_NAME` - Name of your Azure Web App

### 4. Environment Variables Configuration

**For Local Development:**
1. Create `appsettings.Development.json` in the project root (same folder as `appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=online_event_ticketing;Uid=root;Pwd=your-actual-password;"
  }
}
```

2. Add to `.gitignore` to keep secrets out of repository:
```
appsettings.Development.json
```

**Configuration Loading Order:**
ASP.NET Core loads settings in this order (later overrides earlier):
1. `appsettings.json` (base settings)  
2. `appsettings.Development.json` (development environment)
3. Azure App Settings (production environment)

**For Azure Production:**

**In Azure Portal:**
1. Go to your Web App → Configuration → Application settings
2. Add a new application setting:
   - **Name:** `ConnectionStrings__DefaultConnection`
   - **Value:** `Server=your-prod-server;Database=your-prod-db;Uid=your-user;Pwd=your-password;`

**Or via Azure CLI:**
```bash
az webapp config connection-string set --resource-group myResourceGroup --name myApp --connection-string-type SQLServer --settings DefaultConnection="Server=your-prod-server;Database=your-prod-db;Uid=your-user;Pwd=your-password;"
```

**Note:** The double underscore `__` in `ConnectionStrings__DefaultConnection` tells .NET to override the nested `ConnectionStrings:DefaultConnection` value.

## Branch Strategy
- `main`: Production deployments
- Feature branches: CI only (build/test)

## Manual Steps After Setup
1. Create production environment
2. Configure deployment secrets
3. Update deployment commands in workflow
4. Test the pipeline with a sample commit