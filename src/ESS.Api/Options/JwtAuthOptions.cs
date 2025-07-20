﻿namespace ESS.Api.Setup;

public sealed class JwtAuthOptions
{
    public string Issuer { get; init; }
    public string Audiance { get; init; }
    public string Key { get; init; }
    public int ExpirationInMinutes { get; init; }
    public int RefreshTokenExpirationDays { get; init; }
}
