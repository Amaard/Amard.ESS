using ESS.Api.Database.Entities.Settings;

namespace ESS.Api.DTOs.Settings;

public sealed record CreateGeneralSettingsDto
{
    public required GeneralSettingsKey Key { get; init; }
    public string Value { get; init; }
}
