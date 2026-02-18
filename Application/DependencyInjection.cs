using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAttributeService, AttributeService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IProductChatService, ProductChatService>();
        services.AddScoped<ILocationService, LocationService>();
        return services;
    }
}
