namespace AIHub.Api.Models;

public sealed record ModelProfileRequest(string Name, string Provider, string Model, decimal Temperature = 0.2m);

public sealed record ModelProfile(Guid Id, string Name, string Provider, string Model, decimal Temperature, DateTimeOffset CreatedAt);
