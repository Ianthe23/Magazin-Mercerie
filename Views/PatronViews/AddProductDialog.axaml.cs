using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace magazin_mercerie.Views.PatronViews
{
    public partial class AddProductDialog : Window
    {
        private readonly ILog _logger;
        private readonly IService _service;
        
        public Produs? CreatedProduct { get; private set; }

        public AddProductDialog()
        {
            InitializeComponent();
            _logger = LogManager.GetLogger(typeof(AddProductDialog));
            _service = App.ServiceProvider?.GetService<IService>();
            
            _logger?.Info("AddProductDialog initialized");
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            CreatedProduct = null;
            Close();
        }

        private async void CreateProductAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearValidationErrors();
                
                if (!ValidateForm())
                {
                    return;
                }

                ShowLoading(true);
                
                // Get form values
                var name = NameTextBox.Text?.Trim();
                var price = (decimal)PriceNumericUpDown.Value;
                var stock = (decimal)StockNumericUpDown.Value;
                var selectedCategory = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

                // Convert category string to TipProdus enum
                if (!Enum.TryParse<TipProdus>(selectedCategory, out var tipProdus))
                {
                    ShowError("Invalid category selected");
                    return;
                }

                _logger?.Info($"Creating product: {name}, Price: {price}, Stock: {stock}, Category: {tipProdus}");

                // Create product using service
                var createdProduct = await _service.AdaugareProdus(name, tipProdus, price, stock);
                
                if (createdProduct != null)
                {
                    CreatedProduct = createdProduct;
                    
                    ShowSuccess("Product created successfully!");
                    _logger?.Info($"Product created successfully: {name}");
                    
                    // Close dialog after short delay
                    await Task.Delay(1000);
                    Close();
                }
                else
                {
                    ShowError("Failed to create product. Please try again.");
                    _logger?.Error($"Failed to create product: {name}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error creating product: {ex.Message}");
                _logger?.Error("Error creating product", ex);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // Validate name
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowFieldError(NameError, "Product name is required");
                isValid = false;
            }

            // Validate price
            if (PriceNumericUpDown.Value <= 0)
            {
                ShowFieldError(PriceError, "Price must be greater than 0");
                isValid = false;
            }

            // Validate stock
            if (StockNumericUpDown.Value < 0)
            {
                ShowFieldError(StockError, "Stock cannot be negative");
                isValid = false;
            }

            // Validate category
            if (CategoryComboBox.SelectedItem == null)
            {
                ShowError("Please select a category");
                isValid = false;
            }

            return isValid;
        }

        private void ShowFieldError(TextBlock errorTextBlock, string message)
        {
            errorTextBlock.Text = message;
            errorTextBlock.IsVisible = true;
        }

        private void ClearValidationErrors()
        {
            NameError.IsVisible = false;
            PriceError.IsVisible = false;
            StockError.IsVisible = false;
            ErrorPanel.IsVisible = false;
            SuccessPanel.IsVisible = false;
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingPanel.IsVisible = isLoading;
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorPanel.IsVisible = true;
            SuccessPanel.IsVisible = false;
        }

        private void ShowSuccess(string message)
        {
            SuccessMessage.Text = message;
            SuccessPanel.IsVisible = true;
            ErrorPanel.IsVisible = false;
        }
    }
} 