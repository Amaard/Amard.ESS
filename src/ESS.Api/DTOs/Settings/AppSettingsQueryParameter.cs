using ESS.Api.Database.Entities.Settings;
using Microsoft.AspNetCore.Mvc;

namespace ESS.Api.DTOs.Settings;

public sealed record AppSettingsQueryParameter
{
    [FromQuery(Name ="q")]
    public string? Search {  get; set; }
    public AppSettingsType? Type { get; init; }
}
