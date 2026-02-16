# Shopwala - Single Vendor eCommerce Platform

Production-ready ASP.NET Core MVC (.NET 8) eCommerce platform focused on Karachi, Pakistan.

## Features

- **Clean Architecture**: Presentation, Application, Domain, Infrastructure, Shared layers
- **ASP.NET Core Identity**: SuperAdmin, AdminStaff, Buyer roles
- **Email & OTP Verification**: Gmail SMTP, 6-digit OTP on registration
- **Order Confirmation Emails**: Buyers receive email when order is placed
- **Category System**: Unlimited N-level hierarchy
- **Product System**: SKU, variants, dynamic attributes
- **Cart & Orders**: Karachi-only delivery validation
- **Wishlist**: AJAX add/remove
- **Order Workflow**: PendingConfirmation → Confirmed → Shipped → Delivered
- **Delivery Management**: Assign delivery boy, track status
- **Activity Logging**: Middleware-based page/product tracking
- **Admin Dashboard**: Revenue, profit, monthly sales charts

## Setup

1. **SQL Server**: Ensure SQL Server or LocalDB is installed
2. **Connection String**: Update `appsettings.json` if needed
3. **Email**: Email settings are in `appsettings.json` - for production use User Secrets
4. **Run migrations**:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project Presentation
   ```
   Or in Package Manager Console: `Update-Database`
5. **Run the app**:
   ```bash
   dotnet run --project Presentation
   ```

## Default Logins

| Role      | Email                  | Password   |
|-----------|------------------------|------------|
| SuperAdmin| superadmin@shopwala.pk | Admin@123! |
| Buyer     | buyer@shopwala.pk      | Buyer@123! |

## Registration Flow

1. User registers with email & password
2. 6-digit OTP sent to email (valid 10 minutes)
3. User enters OTP on Verify Email page
4. On success → signed in and redirected

## Logo

Place your Shopwala logo at `Presentation/wwwroot/images/logo.png` for branding.
