using System.Collections.ObjectModel;

namespace Demo.WebApi.Shared.Authorization;

public static class AppRoles
{
    public const string Admin = nameof(Admin);
    public const string Basic = nameof(Basic);
    public const string Buyer = nameof(Buyer);
    public const string Owner = nameof(Owner);
    public const string Agent = nameof(Agent);

    public static IReadOnlyList<string> DefaultRoles { get; } = new ReadOnlyCollection<string>(new[]
    {
        Admin,
        Basic,
        Buyer,
        Owner,
        Agent
    });

    public static bool IsDefault(string roleName) => DefaultRoles.Any(r => r == roleName);
}
