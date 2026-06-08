using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ProiectMTP.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OpenBudget_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ProiectMTP.ViewModels.MainWindowViewModel vm)
            {
                vm.OpenBudgetWindow(this);
            }
        }
    }
}