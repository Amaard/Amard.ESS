using ESS.Api.Database.Entities.Settings;

namespace ESS.Api.DTOs.Settings;

public sealed record UpdateGeneralSettingsDto
{
    public string Value { get; init; }
}
