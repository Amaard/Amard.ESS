using ESS.Api.Database.Entities.Settings;

namespace ESS.Api.DTOs.Settings;

public sealed record UpdateAppSettingsDto
{
    public string Value { get; init; }
    public required int Type { get; init; }
    public string? Description { get; init; }
}
