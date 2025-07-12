using Microsoft.AspNetCore.Mvc;

namespace ESS.Api.DTOs.Common;

public record QueryParameter
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
