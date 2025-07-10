using ESS.Api.Database.Entities.Settings;

namespace ESS.Api.DTOs.Settings;

internal static class AppSettingsMapping
{
    public static AppSettingsDto ToDto (this AppSettings generalSettings)
    {
        return new AppSettingsDto
        {
            Id = generalSettings.Id,
            Key = generalSettings.Key,
            Value = generalSettings.Value,
            Type = generalSettings.Type,
            Description = generalSettings.Description,
            CreatedAt = generalSettings.CreatedAt,
            ModifiedAt = generalSettings.ModifiedAt
        };
    }
    public static AppSettings ToEntity(this CreateAppSettingsDto dto)
    {
        AppSettings generalSettings = new()
        {
            Id = $"s_{Guid.CreateVersion7()}",
            Key = dto.Key,
            Type = dto.Type,
            Description = dto.Description,
            Value = dto.Value,
            CreatedAt = DateTime.UtcNow,
        };

        return generalSettings;
    }

    public static void UpdateFromDto(this AppSettings generalSettings, UpdateAppSettingsDto dto)
    {
        generalSettings.Value = dto.Value;
        generalSettings.Type = dto.Type;
        generalSettings.Description = dto.Description;
        generalSettings.ModifiedAt = DateTime.UtcNow;
    }
}
