namespace ESS.Api.Database.Entities.Settings;

public sealed class GeneralSettings
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Key { get; set; }
    public string Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
}
