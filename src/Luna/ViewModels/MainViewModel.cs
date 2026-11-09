using Luna.Mvvm;

namespace Luna.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private string _username = string.Empty;
    private bool _isMicrosoft;
    private string _status = "Choisis un pseudo pour jouer hors-ligne.";

    public MainViewModel()
    {
        PlayCommand = new RelayCommand(Play, () => CanPlay);
        ConnectMicrosoftCommand = new RelayCommand(ConnectMicrosoft);
        UseOfflineCommand = new RelayCommand(() => IsMicrosoft = false);
        UseMicrosoftCommand = new RelayCommand(() => IsMicrosoft = true);
    }

    public RelayCommand PlayCommand { get; }
    public RelayCommand ConnectMicrosoftCommand { get; }
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
            Refresh();
        }
    }

    public bool IsMicrosoft
    {
        get => _isMicrosoft;
        set
        {
            if (!Set(ref _isMicrosoft, value)) return;
            Raise(nameof(IsOffline));
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

    public bool CanPlay => _isMicrosoft ? false : _username.Trim().Length >= 3;

    public string VersionLabel => "0.1.0";

    private void Refresh()
    {
        PlayCommand.RaiseCanExecuteChanged();
        Raise(nameof(CanPlay));

        if (_isMicrosoft)
            Status = "Compte Microsoft : non connecte.";
        else if (CanPlay)
            Status = "Pret a lancer.";
        else
            Status = "Pseudo trop court (3 caracteres minimum).";
    }

    private void Play()
    {
        Status = "Lancement pas encore implemente.";
    }

    private void ConnectMicrosoft()
    {
        IsMicrosoft = true;
        Status = "Connexion Microsoft pas encore implementee.";
    }
}