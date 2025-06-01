using Avalonia.Controls;
using Avalonia.Interactivity;

namespace magazin_mercerie.Views.ClientViews
{
    public partial class CartDialog : Window
    {
        public CartDialog()
        {
            InitializeComponent();
        }
        
        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 