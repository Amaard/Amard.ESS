using ESS.Api.Database.Entities.Settings;
using ESS.Api.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace ESS.Api.DTOs.Settings;

public sealed record AppSettingsQueryParameter : QueryParameter
{
    public AppSettingsType? Type { get; init; }
}
