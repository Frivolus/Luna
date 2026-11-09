using CmlLib.Core.Auth;
using Luna.Services;

namespace Luna.Models;

public static class SessionFactory
{
    public static MSession Offline(string username)
    {
        var session = MSession.CreateOfflineSession(username);
        session.UUID = OfflineIdentity.UuidFor(username);
        return session;
    }
}