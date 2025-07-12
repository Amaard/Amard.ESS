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
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ESS.Api.Controllers;

[ApiController]
[Route("settings")]
public sealed class AppSettingsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAppSettings(
        [FromQuery] AppSettingsQueryParameter query,
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

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeCollectionData(appSettings, query.Fields),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppSettings(
        string id,
        string? fields,
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

        ExpandoObject ShapedappSetting = dataShapingService.ShapeData(appSetting, fields);

        return Ok(ShapedappSetting);
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

        AppSettingsDto AppSettingsDto = appSetting.ToDto();

        return CreatedAtAction(nameof(GetAppSettings), new { id = AppSettingsDto.Id }, AppSettingsDto);

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

}
