using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Controls
{
    /// <summary>
    /// Interaction logic for QuickActionsPanel.xaml
    /// </summary>
    public partial class QuickActionsPanel : UserControl
    {
        public QuickActionsPanel()
        {
            InitializeComponent();
            DataContext = new QuickActionsViewModel();
        }
    }
}
