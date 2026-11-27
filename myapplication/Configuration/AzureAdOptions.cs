namespace myapplication.Configuration;

public sealed class AzureAdOptions
{
    public string Instance { get; init; } = "https://sts.windows.net/";

    public string TenantId { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string? Audience { get; init; }

    public string Authority => $"{Instance.TrimEnd('/')}/{TenantId}/";
}
