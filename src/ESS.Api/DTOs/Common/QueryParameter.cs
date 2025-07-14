using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using ESS.Api.Services;

namespace ESS.Api.DTOs.Common;

public record QueryParameter
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public string? Sort { get; init; }
    public string? Fields { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    [FromHeader(Name ="Accept")]
    public string? Accept { get; init; }

    public bool IncludesLinks => 
        MediaTypeHeaderValue.TryParse(Accept, out MediaTypeHeaderValue? mediaType) &&
        mediaType.SubTypeWithoutSuffix.HasValue &&
        mediaType.SubTypeWithoutSuffix.Value.Contains(CustomeMediaTypeNames.Application.HateoasSubType);
}
