using BusBuddy.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(StudentListViewModel studentListViewModel)
    {
        InitializeComponent();
        this.WindowState = WindowState.Maximized;
        this.WindowStyle = WindowStyle.SingleBorderWindow;
        this.ResizeMode = ResizeMode.CanResize;

        // Set DataContext for the student list view
        var studentListTab = (TabItem)this.FindName("StudentListTab");
        if (studentListTab != null)
        {
            studentListTab.DataContext = studentListViewModel;
        }


        // Set DataContext for MainWindow if needed
        if (Application.Current is App app && app.Services != null)
        {
            // If you have a MainWindowViewModel, you can set it here
            // DataContext = app.Services.GetRequiredService<MainWindowViewModel>();
        }
    }
}