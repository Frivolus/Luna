using CmlLib.Core.Auth;
using Luna.Models;
using Luna.Mvvm;
using Luna.Services;

namespace Luna.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly ManifestService _manifests = new();
    private readonly AccountStore _accounts = new();
    private readonly MicrosoftAuthService _microsoft = new();

    private string _username = string.Empty;
    private bool _isMicrosoft;
    private string _status = "Choisis un pseudo pour jouer hors-ligne.";
    private string _versionText = "Chargement du manifeste...";
    private Manifest? _manifest;
    private MSession? _session;

    public MainViewModel()
    {
        PlayCommand = new RelayCommand(Play, () => CanPlay);
        ConnectMicrosoftCommand = new AsyncRelayCommand(ConnectMicrosoftAsync, () => _session is null);
        UseOfflineCommand = new RelayCommand(() => IsMicrosoft = false);
        UseMicrosoftCommand = new RelayCommand(() => IsMicrosoft = true);

        var saved = _accounts.Load();
        if (saved is not null && saved.Kind == AccountKind.Offline && saved.Username.Length >= 3)
        {
            _username = saved.Username;
            Raise(nameof(Username));
            Raise(nameof(UuidText));
        }
        else if (saved is not null && saved.Kind == AccountKind.Microsoft)
        {
            _isMicrosoft = true;
            _ = RestoreMicrosoftAsync();
        }

        _ = LoadManifestAsync();
    }

    public RelayCommand PlayCommand { get; }
    public AsyncRelayCommand ConnectMicrosoftCommand { get; }
    public RelayCommand UseOfflineCommand { get; }
    public RelayCommand UseMicrosoftCommand { get; }

    public Wpf.Ui.Controls.ControlAppearance OfflineAppearance
        => _isMicrosoft ? Wpf.Ui.Controls.ControlAppearance.Secondary : Wpf.Ui.Controls.ControlAppearance.Primary;

    public Wpf.Ui.Controls.ControlAppearance MicrosoftAppearance
        => _isMicrosoft ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;

    public string Username
    {
        get => _username;
        set
        {
            if (!Set(ref _username, value)) return;
            Raise(nameof(UuidText));
            Persist();
            Refresh();
        }
    }

    public string UuidText
    {
        get
        {
            if (_isMicrosoft)
                return _session is null ? "Aucun compte Microsoft" : "Connecte : " + _session.Username;

            var name = _username.Trim();
            return name.Length >= 3 ? "UUID " + OfflineIdentity.UuidFor(name) : "UUID —";
        }
    }

    public bool IsMicrosoft
    {
        get => _isMicrosoft;
        set
        {
            if (!Set(ref _isMicrosoft, value)) return;
            Raise(nameof(IsOffline));
            Raise(nameof(UuidText));
            Raise(nameof(OfflineAppearance));
            Raise(nameof(MicrosoftAppearance));
            Refresh();
        }
    }

    public bool IsOffline
    {
        get => !_isMicrosoft;
        set
        {
            if (value && _isMicrosoft) IsMicrosoft = false;
        }
    }

    public string Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    public string VersionText
    {
        get => _versionText;
        private set => Set(ref _versionText, value);
    }

    public string VersionLabel => AppInfo.Version;

    public bool CanPlay => _manifest is not null
        && (_isMicrosoft ? _session is not null : _username.Trim().Length >= 3);

    private void Persist()
    {
        if (_isMicrosoft)
        {
            if (_session is not null)
                _accounts.Save(new Account(AccountKind.Microsoft, _session.Username ?? string.Empty, _session.UUID));
            return;
        }

        var name = _username.Trim();
        if (name.Length < 3) return;
        _accounts.Save(new Account(AccountKind.Offline, name, OfflineIdentity.UuidFor(name)));
    }

    private async Task RestoreMicrosoftAsync()
    {
        Status = "Restauration de la session Microsoft...";
        var session = await _microsoft.TrySilentAsync(CancellationToken.None);
        ApplySession(session);
    }

    private async Task ConnectMicrosoftAsync()
    {
        Status = "Ouverture de la fenetre de connexion Microsoft...";
        var session = await _microsoft.SignInAsync(CancellationToken.None);
        ApplySession(session);
    }

    private void ApplySession(MSession? session)
    {
        _session = session;
        Raise(nameof(UuidText));

        if (session is null)
        {
            Status = "Connexion Microsoft echouee.";
        }
        else
        {
            IsMicrosoft = true;
            Persist();
            Status = "Connecte en tant que " + (session.Username ?? "?") + ".";
        }

        Refresh();
    }

    private async Task LoadManifestAsync()
    {
        var result = await _manifests.LoadAsync(AppInfo.ManifestUrl, CancellationToken.None);

        if (!result.Ok || result.Value is null)
        {
            _manifest = null;
            VersionText = "Manifeste indisponible";
            Status = result.Error ?? "Manifeste indisponible.";
            Refresh();
            return;
        }

        _manifest = result.Value;
        VersionText = $"{_manifest.Name} — Minecraft {_manifest.McVersion} ({_manifest.Loader})";
        Refresh();
    }

    private void Refresh()
    {
        PlayCommand.RaiseCanExecuteChanged();
        ConnectMicrosoftCommand.RaiseCanExecuteChanged();
        Raise(nameof(CanPlay));

        if (_manifest is null) return;

        if (_isMicrosoft)
            Status = _session is null ? "Compte Microsoft : non connecte." : "Pret a lancer.";
        else if (CanPlay)
            Status = "Pret a lancer.";
        else
            Status = "Pseudo trop court (3 caracteres minimum).";
    }

    private void Play()
    {
        Status = "Lancement pas encore implemente.";
    }
}