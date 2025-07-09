using ESS.Api.Database;
using ESS.Api.Database.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESS.Api.DTOs.Settings;
using Microsoft.AspNetCore.JsonPatch;

namespace ESS.Api.Controllers;

[ApiController]
[Route("general-settings")]
public sealed class GeneralSettingsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GeneralSettingsCollectionDto>> GetGeneralSettings()
    {
        List<GeneralSettingsDto> generalSettingsList = await dbContext
            .GeneralSettings
            .Select(GeneralSettingsQueries.ProjectToDto()).ToListAsync();

        var generalSettingsCollectionDto =
         new GeneralSettingsCollectionDto
         {
             Data = generalSettingsList
         };

        return Ok(generalSettingsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GeneralSettingsDto>> GetGeneralSettings(string id)
    {
        GeneralSettingsDto? generalSetting = await dbContext
            .GeneralSettings
            .Where(h => h.Id == id)
            .Select(GeneralSettingsQueries.ProjectToDto()).FirstOrDefaultAsync();

        if (generalSetting is null)
        {
            return NotFound();
        }

        return Ok(generalSetting);
    }

    [HttpPost]
    public async Task<ActionResult<GeneralSettingsDto>> CreateGeneralSettings(CreateGeneralSettingsDto createGeneralSettingsDto)
    {
        GeneralSettings generalSetting = createGeneralSettingsDto.ToEntity();

        if (!GeneralSettingsKeyExtensions.IsValidKey(generalSetting.Key))
        {
            return BadRequest("Invalid Settings Key");
        }

        if (await dbContext.GeneralSettings.AnyAsync(s=> s.Key == generalSetting.Key))
        {
            return Conflict($"The Setting '{generalSetting.Key}' already exists");
        }

        dbContext.GeneralSettings.Add(generalSetting);

        await dbContext.SaveChangesAsync();

        GeneralSettingsDto generalSettingsDto = generalSetting.ToDto();

        return CreatedAtAction(nameof(GetGeneralSettings), new { id = generalSettingsDto.Id }, generalSettingsDto);

    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateGeneralSettings(string id, UpdateGeneralSettingsDto updateGeneralSettingsDto)
    {
        GeneralSettings? generalSettings = await dbContext.GeneralSettings.FirstOrDefaultAsync(h => h.Id == id);

        if (generalSettings is null)
        {
            return NotFound();
        }

        generalSettings.UpdateFromDto(updateGeneralSettingsDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchGeneralSettings(string id, JsonPatchDocument<GeneralSettingsDto> patchDocument)
    {
        GeneralSettings? generalSettings = await dbContext.GeneralSettings.FirstOrDefaultAsync(h => h.Id == id);

        if (generalSettings is null)
        {
            return NotFound();
        }

        GeneralSettingsDto generalSettingsDto = generalSettings.ToDto();

        patchDocument.ApplyTo(generalSettingsDto, ModelState);

        if (!TryValidateModel(generalSettingsDto))
        {
            return ValidationProblem(ModelState);
        }

        generalSettings.Value = generalSettingsDto.Value;
        generalSettings.ModifiedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGeneralSettings(string id)
    {
        GeneralSettings? generalSettings = await dbContext.GeneralSettings.FirstOrDefaultAsync(g => g.Id == id);

        if (generalSettings is null)
        {
            return NotFound();
        }

        dbContext.GeneralSettings.Remove(generalSettings);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

}
