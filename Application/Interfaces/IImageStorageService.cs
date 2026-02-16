namespace Application.Interfaces;

public interface IImageStorageService
{
    Task<string?> SaveCategoryImageAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    Task<string?> SaveProductImageAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string? imageUrl, CancellationToken cancellationToken = default);
}
