using Avalonia.Controls;
using Avalonia.Interactivity;
using log4net;

namespace magazin_mercerie.Views.PatronViews
{
    public partial class ConfirmDeleteDialog : Window
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ConfirmDeleteDialog));
        public bool WasConfirmed { get; private set; }

        public ConfirmDeleteDialog(string message = "Are you sure you want to delete this item?")
        {
            InitializeComponent();
            MessageTextBlock.Text = message;
            _logger.Debug("ConfirmDeleteDialog initialized");
        }

        private void CancelDelete(object? sender, RoutedEventArgs e)
        {
            _logger.Debug("Delete operation cancelled by user");
            WasConfirmed = false;
            Close();
        }

        private void ConfirmDelete(object? sender, RoutedEventArgs e)
        {
            _logger.Debug("Delete operation confirmed by user");
            WasConfirmed = true;
            Close();
        }
    }
} 