using Domain.Enums;

namespace Shared.DTOs;

public record CategoryAttributeDto(
    int Id,
    string Name,
    AttributeFieldType FieldType,
    bool IsRequired,
    List<string> Options
);
