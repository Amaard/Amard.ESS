using System.Dynamic;
using ESS.Api.Database;
using ESS.Api.Database.Entities.Settings;
using ESS.Api.DTOs.Common;
using ESS.Api.DTOs.Settings;
using ESS.Api.Services;
using ESS.Api.Services.Sorting;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ESS.Api.Controllers;

[ApiController]
[Route("settings")]
public sealed class AppSettingsController(ApplicationDbContext dbContext, LinkService linkService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAppSettings(
        [FromQuery] AppSettingsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService)
    {
        if (!sortMappingProvider.ValidateMappings<AppSettingsDto, AppSettings>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{query.Sort}'");
        }

        if (!dataShapingService.Validate<AppSettingsDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided Fields aren't valid: '{query.Fields}'");
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            query.Search = query.Search.Trim().ToLower();
        }

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<AppSettingsDto, AppSettings>();

        IQueryable<AppSettingsDto> appSettingsQuery = dbContext
            .AppSettings
            .Where(s => query.Search == null ||
                        s.Key.ToLower().Contains(query.Search) ||
                        s.Description != null && s.Description.ToLower().Contains(query.Search))
            .Where(s => query.Type == null || s.Type == query.Type)
            .ApplySort(query.Sort, sortMappings)
            .Select(AppSettingsQueries.ProjectToDto());

        int totalCount = await appSettingsQuery.CountAsync();

        var appSettings = await appSettingsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        bool includeLinks = query.Accept == CustomeMediaTypeNames.Application.HateoasJson;

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeCollectionData(
                appSettings,
                query.Fields,
                includeLinks ? s => CreateLinksForAppSettings(s.Id, query.Fields) : null),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
        };
        if (includeLinks)
        {
            paginationResult.Links = CreateLinksForAppSettings(
                    query,
                    paginationResult.HasNextPage,
                    paginationResult.HasPreviousPage);
        }
        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppSettings(
        string id,
        string? fields,
        [FromHeader(Name="Accept")]
        string? accept,
        DataShapingService dataShapingService)
    {
        if (!dataShapingService.Validate<AppSettingsDto>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided Fields aren't valid: '{fields}'");
        }

        AppSettingsDto? appSetting = await dbContext
            .AppSettings
            .Where(h => h.Id == id)
            .Select(AppSettingsQueries.ProjectToDto()).FirstOrDefaultAsync();

        if (appSetting is null)
        {
            return NotFound();
        }

        ExpandoObject ShapedAppSetting = dataShapingService.ShapeData(appSetting, fields);

        if (accept == CustomeMediaTypeNames.Application.HateoasJson)
        {
            List<LinkDto> links = CreateLinksForAppSettings(id, fields);
            ShapedAppSetting.TryAdd("links", links);
        }

        return Ok(ShapedAppSetting);
    }

    [HttpPost]
    public async Task<ActionResult<AppSettingsDto>> CreateAppSettings(
        CreateAppSettingsDto createAppSettingsDto,
        IValidator<CreateAppSettingsDto> validator)
    {
        await validator.ValidateAndThrowAsync(createAppSettingsDto);

        AppSettings appSetting = createAppSettingsDto.ToEntity();

        if (await dbContext.AppSettings.AnyAsync(s => s.Key == appSetting.Key))
        {
            return Problem(detail: $"The Setting '{appSetting.Key}' already exists",
                           statusCode: StatusCodes.Status409Conflict);
        }

        dbContext.AppSettings.Add(appSetting);

        await dbContext.SaveChangesAsync();

        AppSettingsDto appSettingsDto = appSetting.ToDto();

        appSettingsDto.Links = CreateLinksForAppSettings(appSetting.Id, null);

        return CreatedAtAction(nameof(GetAppSettings), new { id = appSettingsDto.Id }, appSettingsDto);

    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAppSettings(string id, UpdateAppSettingsDto updateAppSettingsDto)
    {
        AppSettings? AppSettings = await dbContext.AppSettings.FirstOrDefaultAsync(h => h.Id == id);

        if (AppSettings is null)
        {
            return NotFound();
        }

        AppSettings.UpdateFromDto(updateAppSettingsDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchAppSettings(string id, JsonPatchDocument<AppSettingsDto> patchDocument)
    {
        AppSettings? AppSettings = await dbContext.AppSettings.FirstOrDefaultAsync(h => h.Id == id);

        if (AppSettings is null)
        {
            return NotFound();
        }

        AppSettingsDto AppSettingsDto = AppSettings.ToDto();

        patchDocument.ApplyTo(AppSettingsDto, ModelState);

        if (!TryValidateModel(AppSettingsDto))
        {
            return ValidationProblem(ModelState);
        }

        AppSettings.Value = AppSettingsDto.Value;
        AppSettings.ModifiedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAppSettings(string id)
    {
        AppSettings? AppSettings = await dbContext.AppSettings.FirstOrDefaultAsync(g => g.Id == id);

        if (AppSettings is null)
        {
            return NotFound();
        }

        dbContext.AppSettings.Remove(AppSettings);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private List<LinkDto> CreateLinksForAppSettings(
        AppSettingsQueryParameters parameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetAppSettings), "self" , HttpMethods.Get , new
            {
                page     = parameters.Page,
                pageSize = parameters.PageSize,
                fields   = parameters.Fields,
                q        = parameters.Search,
                sort     = parameters.Sort,
                type     = parameters.Type
            }),
            linkService.Create(nameof(CreateAppSettings), "create" , HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetAppSettings), "next-page", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type
            }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(nameof(GetAppSettings), "prev-page", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type
            }));
        }

        return links;
    }
    private List<LinkDto> CreateLinksForAppSettings(string id, string? fields)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetAppSettings), "self" , HttpMethods.Get , new {id , fields} ),
            linkService.Create(nameof(UpdateAppSettings), "update" , HttpMethods.Put , new {id} ),
            linkService.Create(nameof(PatchAppSettings), "partial-update" , HttpMethods.Patch , new {id} ),
            linkService.Create(nameof(DeleteAppSettings), "delete" , HttpMethods.Delete , new {id} ),
        ];
        return links;
    }

}
