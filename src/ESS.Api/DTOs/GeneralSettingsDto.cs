namespace ESS.Api.DTOs;

public sealed record GeneralSettingsDto
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public string Value { get; init; }
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; init; }
}

public sealed record GeneralSettingsCollectionDto
{
    public List<GeneralSettingsDto> Data { get; init; }
}
