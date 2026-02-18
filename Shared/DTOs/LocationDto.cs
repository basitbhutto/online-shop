namespace Shared.DTOs;

public record LocationOptionDto(long Id, string Name, long? ParentId, decimal? Latitude, decimal? Longitude);

public record LocationWithPathDto(long Id, string Name, string FullPath);
