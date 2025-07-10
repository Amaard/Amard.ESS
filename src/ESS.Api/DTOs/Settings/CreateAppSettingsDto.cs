using System.ComponentModel.DataAnnotations;

namespace ESS.Api.DTOs.Settings;

public sealed record CreateAppSettingsDto
{
    public required string Key { get; init; }
    public string Value { get; init; }
    public required int Type { get; init; }
    public string? Description { get; init; }
}
