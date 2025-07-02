namespace ESS.Api.Database.Entities.Settings;

public sealed class GeneralSettings
{
    public string Id { get; set; }
    public GeneralSettingsKey Key { get; set; }
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
}
