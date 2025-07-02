using ESS.Api.Database.Entities.Settings;

namespace ESS.Api.DTOs.Settings;

internal static class GeneralSettingsMapping
{
    public static GeneralSettingsDto ToDto (this GeneralSettings generalSettings)
    {
        return new GeneralSettingsDto
        {
            Id = generalSettings.Id,
            Key = generalSettings.Key,
            Value = generalSettings.Value,
            CreatedAt = generalSettings.CreatedAt,
            ModifiedAt = generalSettings.ModifiedAt
        };
    }
    public static GeneralSettings ToEntity(this CreateGeneralSettingsDto dto)
    {
        GeneralSettings generalSettings = new()
        {
            Id = $"s_{Guid.CreateVersion7()}",
            Key = dto.Key,
            Value = dto.Value,
            CreatedAt = DateTime.UtcNow,
        };

        return generalSettings;
    }

    public static void UpdateFromDto(this GeneralSettings generalSettings, UpdateGeneralSettingsDto dto)
    {
        generalSettings.Value = dto.Value;
        generalSettings.ModifiedAt = DateTime.UtcNow;
    }
}
