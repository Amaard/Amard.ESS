using ESS.Api.Database;
using ESS.Api.Database.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESS.Api.DTOs.Settings;

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
}
