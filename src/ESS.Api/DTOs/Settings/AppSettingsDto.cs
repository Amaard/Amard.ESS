﻿using ESS.Api.Database.Entities.Settings;

namespace ESS.Api.DTOs.Settings;

public sealed record AppSettingsDto
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public string Value { get; init; }
    public required AppSettingsType Type { get; init; }
    public string? Description { get; init; }
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; init; }
}
