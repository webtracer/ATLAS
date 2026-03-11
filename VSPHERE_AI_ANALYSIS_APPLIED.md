# vSphere AI Analysis Tool - Applied to Atlas Project

## Summary

The vSphere AI Analysis tool has been successfully applied to the Atlas project located at:
`C:\Users\randybi\OneDrive - AMDOCS\Documents\Coding Projects\C#\Atlas`

All changes from the Amdocs.Atlas project have been replicated here.

## Changes Applied

### ✅ 1. Core Entity Updates
- **File**: `Amdocs.Atlas.Core\Entities\Vcenter.cs`
- **Changes**: Added `Host`, `Port`, and `SslVerify` fields

### ✅ 2. DTOs Created
- **File**: `Amdocs.Atlas.Core\DTOs\VSphereAnalysisRequest.cs`
- **File**: `Amdocs.Atlas.Core\DTOs\VSphereAnalysisResult.cs`
- **File**: `Amdocs.Atlas.Core\DTOs\VSphereConnectionInfo.cs`

### ✅ 3. Services Created
- **File**: `Amdocs.Atlas.Api\Services\IVSphereClientService.cs`
- **File**: `Amdocs.Atlas.Api\Services\VSphereClientService.cs` - vSphere REST API client
- **File**: `Amdocs.Atlas.Api\Services\IAIAnalysisService.cs`
- **File**: `Amdocs.Atlas.Api\Services\AIAnalysisService.cs` - CoPilot AI integration
- **File**: `Amdocs.Atlas.Api\Services\VSphereAnalysisService.cs` - Orchestration service

### ✅ 4. API Controller Created
- **File**: `Amdocs.Atlas.Api\Controllers\VSphereAnalysisController.cs`
- **Endpoints**:
  - `POST /api/vsphereanalysis/vcenters/{id}/clusters` - Get clusters
  - `POST /api/vsphereanalysis/analyze` - Run analysis
  - `POST /api/vsphereanalysis/vcenters/{id}/clusters/{name}/analyze/performance`
  - `POST /api/vsphereanalysis/vcenters/{id}/clusters/{name}/analyze/capacity`
  - `POST /api/vsphereanalysis/vcenters/{id}/clusters/{name}/analyze/logs`

### ✅ 5. Blazor UI Created
- **File**: `Amdocs.Atlas.Web\Pages\VSphereAnalysis.razor`
- **Route**: `/vcenters/{vcenterId}/analysis`

### ✅ 6. Configuration Updated
- **File**: `Amdocs.Atlas.Api\Program.cs` - Services registered
- **File**: `Amdocs.Atlas.Api\appsettings.json` - CoPilot configuration added

## Next Steps

### 1. Database Migration

You need to add the new columns to the `Vcenters` table. Run this SQL:

```sql
-- Add the new columns to Vcenters table
ALTER TABLE `Vcenters` 
ADD COLUMN `Host` longtext CHARACTER SET utf8mb4 NULL AFTER `IpAddress`,
ADD COLUMN `Port` int NOT NULL DEFAULT 443 AFTER `Host`,
ADD COLUMN `SslVerify` tinyint(1) NOT NULL DEFAULT 0 AFTER `Port`;

-- Update existing records to set Host from IpAddress if needed
UPDATE `Vcenters` 
SET `Host` = `IpAddress` 
WHERE `Host` = '' OR `Host` IS NULL;
```

### 2. Update Existing vCenter Records

After adding the columns, update existing vCenter records:

```sql
UPDATE `Vcenters` 
SET `Host` = `IpAddress`, `Port` = 443, `SslVerify` = 0 
WHERE `Host` = '' OR `Host` IS NULL;
```

### 3. Configure CoPilot API Key (Optional)

Add your CoPilot/Azure OpenAI API key to `appsettings.json` or use User Secrets:

```bash
dotnet user-secrets set "CoPilot:ApiKey" "your-api-key-here" --project "Amdocs.Atlas.Api"
```

Or edit `appsettings.json`:
```json
{
  "CoPilot": {
    "Endpoint": "https://api.openai.com/v1/chat/completions",
    "ApiKey": "your-api-key-here",
    "Model": "gpt-4"
  }
}
```

### 4. Test the Implementation

1. **Start the API**:
   ```bash
   dotnet run --project "Amdocs.Atlas.Api"
   ```

2. **Start the Web UI**:
   ```bash
   dotnet run --project "Amdocs.Atlas.Web"
   ```

3. **Navigate to**: `http://localhost:{port}/vcenters/{vcenterId}/analysis`

## Differences from Amdocs.Atlas Project

This Atlas project has some differences:
- Uses **.NET 8.0** (vs .NET 9.0 in Amdocs.Atlas)
- Has **AutoMapper** integration
- Has **Swagger** configured
- Runs as **Windows Service**
- Has **Repository pattern** implemented
- Uses different **API port** (5186 vs 5094)

All vSphere AI Analysis functionality is identical and compatible.

## API Endpoints

All endpoints require credentials in the request body (never stored):

### Get Clusters
```
POST /api/vsphereanalysis/vcenters/{vcenterId}/clusters
Body: { "username": "...", "password": "..." }
```

### Run Analysis
```
POST /api/vsphereanalysis/analyze
Body: {
  "vcenterId": 1,
  "username": "...",
  "password": "...",
  "clusterName": "Cluster-1",  // optional
  "analysisType": "performance"  // "performance", "capacity", "logs", "general"
}
```

## Security

✅ **Credentials are never stored** - Only accepted in API requests  
✅ **Per-user credentials** - Each user provides their own  
✅ **No root credentials** - System does not store admin credentials  
✅ **Session-based** - vSphere sessions are created and destroyed per request

## Status

✅ All code files created  
✅ Services registered  
✅ Configuration updated  
⏳ Database migration pending (SQL provided above)  
⏳ CoPilot API key configuration pending (optional)

---

**Applied Date**: 2025-01-XX  
**Project**: Atlas (C:\Users\randybi\OneDrive - AMDOCS\Documents\Coding Projects\C#\Atlas)  
**Status**: Ready for database migration and testing
