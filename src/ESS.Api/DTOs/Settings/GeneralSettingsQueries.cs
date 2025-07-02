using System.Linq.Expressions;
using ESS.Api.Database.Entities.Settings;

namespace ESS.Api.DTOs.Settings;

internal static class GeneralSettingsQueries
{
    public static Expression<Func<GeneralSettings, GeneralSettingsDto>> ProjectToDto()
    {
        return h => new GeneralSettingsDto
        {
            Id = h.Id,
            Key = h.Key,
            Value = h.Value,
            CreatedAt = h.CreatedAt,
            ModifiedAt = h.ModifiedAt

        };
    }
}
