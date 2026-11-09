using System.Windows;
using Luna.ViewModels;

namespace Luna;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}