# Fix Update-Database / System.Runtime 10.0.0.0 Error

You only have **.NET 10 SDK** installed. The EF Core tools and this solution target **.NET 8**, which causes the `System.Runtime, Version=10.0.0.0` error.

## Solution: Install .NET 8 SDK

1. **Download .NET 8 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0  
   - Click "Download .NET SDK x64" (or ARM if applicable)

2. **Install it**, then restart Visual Studio / your terminal.

3. **Install EF Core 8 tools**:
   ```powershell
   dotnet tool uninstall --global dotnet-ef
   dotnet tool install --global dotnet-ef --version 8.0.11
   ```

4. **Delete cached build output** (close Visual Studio first):
   ```powershell
   cd "D:\Project\online-shop\online-shop"
   Remove-Item -Recurse -Force *\bin, *\obj -ErrorAction SilentlyContinue
   ```

5. **Run migrations** from Package Manager Console:
   ```powershell
   Update-Database -Project Infrastructure -StartupProject Presentation
   ```

   Or from command line:
   ```powershell
   dotnet ef database update --project Infrastructure --startup-project Presentation
   ```

## Optional: Force .NET 8 SDK

After installing .NET 8, add `global.json` in the solution folder to prefer .NET 8:

```json
{
  "sdk": {
    "version": "8.0.0",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```
