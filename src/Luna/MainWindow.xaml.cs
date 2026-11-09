using Luna.ViewModels;
using Wpf.Ui.Controls;

namespace Luna;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}