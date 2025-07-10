using ESS.Api.Database;
using ESS.Api.Database.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESS.Api.DTOs.Settings;
using Microsoft.AspNetCore.JsonPatch;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ESS.Api.Controllers;

[ApiController]
[Route("settings")]
public sealed class AppSettingsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AppSettingsCollectionDto>> GetAppSettings()
    {
        List<AppSettingsDto> AppSettingsList = await dbContext
            .AppSettings
            .Select(AppSettingsQueries.ProjectToDto()).ToListAsync();

        var AppSettingsCollectionDto =
         new AppSettingsCollectionDto
         {
             Data = AppSettingsList
         };

        return Ok(AppSettingsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppSettingsDto>> GetAppSettings(string id)
    {
        AppSettingsDto? generalSetting = await dbContext
            .AppSettings
            .Where(h => h.Id == id)
            .Select(AppSettingsQueries.ProjectToDto()).FirstOrDefaultAsync();

        if (generalSetting is null)
        {
            return NotFound();
        }

        return Ok(generalSetting);
    }

    [HttpPost]
    public async Task<ActionResult<AppSettingsDto>> CreateAppSettings(
        CreateAppSettingsDto createAppSettingsDto,
        IValidator<CreateAppSettingsDto> validator)
    {
        await validator.ValidateAndThrowAsync(createAppSettingsDto);

        AppSettings generalSetting = createAppSettingsDto.ToEntity();

        if (await dbContext.AppSettings.AnyAsync(s => s.Key == generalSetting.Key))
        {
            return Problem(detail: $"The Setting '{generalSetting.Key}' already exists",
                           statusCode: StatusCodes.Status409Conflict);
        }

        dbContext.AppSettings.Add(generalSetting);

        await dbContext.SaveChangesAsync();

        AppSettingsDto AppSettingsDto = generalSetting.ToDto();

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
