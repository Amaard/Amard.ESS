using ESS.Api.Database;
using ESS.Api.Database.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using ESS.Api.DTOs;
using Microsoft.EntityFrameworkCore;

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
            .Select(h => new GeneralSettingsDto
            {
                Id = h.Id,
                Key = h.Key,
                Value = h.Value,
                CreatedAt = h.CreatedAt,
                ModifiedAt = h.ModifiedAt

            }).ToListAsync();

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
            .Select(h => new GeneralSettingsDto
            {
                Id = h.Id,
                Key = h.Key,
                Value = h.Value,
                CreatedAt = h.CreatedAt,
                ModifiedAt = h.ModifiedAt

            }).FirstOrDefaultAsync();

        if (generalSetting is null)
        {
            return NotFound();
        }

        return Ok(generalSetting);
    }
}
