using Avalonia.Controls;
using Avalonia.Interactivity;

namespace magazin_mercerie.Views.ClientViews
{
    public partial class OrderDetailsWindow : Window
    {
        public OrderDetailsWindow()
        {
            InitializeComponent();
        }
        
        public OrderDetailsWindow(Comanda order) : this()
        {
            DataContext = order;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 