namespace Luna.Models;

public enum AccountKind
{
    Offline,
    Microsoft
}

public sealed record Account(AccountKind Kind, string Username, string? Uuid);