using BusBuddy.WPF.ViewModels;
using System.Windows;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.WindowState = WindowState.Maximized;
        this.WindowStyle = WindowStyle.SingleBorderWindow;
        this.ResizeMode = ResizeMode.CanResize;
    }
}