using Domain.Enums;

namespace Shared.DTOs;

public record CategoryAttributeDto(
    long Id,
    string Name,
    AttributeFieldType FieldType,
    bool IsRequired,
    List<string> Options
);
