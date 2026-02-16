using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Services;

public class ImageStorageService : IImageStorageService
{
    private readonly IWebHostEnvironment _env;
    private const string CategoriesFolder = "uploads/categories";
    private const string ProductsFolder = "uploads/products";

    public ImageStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string?> SaveCategoryImageAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(ext))
            ext = ".jpg";
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", CategoriesFolder);
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, uniqueName);
        await using (var fs = new FileStream(fullPath, FileMode.Create))
            await stream.CopyToAsync(fs, cancellationToken);
        return $"/{CategoriesFolder}/{uniqueName}";
    }

    public async Task<string?> SaveProductImageAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(ext))
            ext = ".jpg";
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", ProductsFolder);
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, uniqueName);
        await using (var fs = new FileStream(fullPath, FileMode.Create))
            await stream.CopyToAsync(fs, cancellationToken);
        return $"/{ProductsFolder}/{uniqueName}";
    }

    public Task DeleteImageAsync(string? imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("/uploads/")) return Task.CompletedTask;
        try
        {
            var path = Path.Combine(_env.WebRootPath ?? "wwwroot", imageUrl.TrimStart('/'));
            if (File.Exists(path)) File.Delete(path);
        }
        catch { /* ignore */ }
        return Task.CompletedTask;
    }
}
