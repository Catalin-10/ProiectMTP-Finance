using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ProiectMTP.Views
{
    public partial class BudgetWindow : Window
    {
        public BudgetWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}