using System.Linq.Expressions;
using ESS.Api.Database.Entities.Settings;

namespace ESS.Api.DTOs.Settings;

internal static class GeneralSettingsQueries
{
    public static Expression<Func<GeneralSettings, GeneralSettingsDto>> ProjectToDto()
    {
        return s => new GeneralSettingsDto
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            Description = s.Description,
            CreatedAt = s.CreatedAt,
            ModifiedAt = s.ModifiedAt

        };
    }
}
