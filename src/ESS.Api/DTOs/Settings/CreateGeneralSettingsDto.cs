using System.ComponentModel.DataAnnotations;

namespace ESS.Api.DTOs.Settings;

public sealed record CreateGeneralSettingsDto
{
    public required string Key { get; init; }
    public string Value { get; init; }
    public string? Description { get; init; }
}
