using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;

namespace Luna.Services;

public sealed class MicrosoftAuthService
{
    private readonly JELoginHandler _handler = JELoginHandlerBuilder.BuildDefault();

    public Task<MSession> SignInAsync(CancellationToken cancellationToken)
        => _handler.AuthenticateInteractively(cancellationToken);

    public async Task<MSession?> TrySilentAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _handler.AuthenticateSilently(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }
}