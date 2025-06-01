using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace magazin_mercerie.Views.PatronViews
{
    public partial class EditProductDialog : Window
    {
        private readonly ILog _logger;
        private readonly IService _service;
        private readonly Produs _originalProduct;
        
        public bool WasUpdated { get; private set; }

        public EditProductDialog(Produs product)
        {
            InitializeComponent();
            _logger = LogManager.GetLogger(typeof(EditProductDialog));
            _service = App.ServiceProvider?.GetService<IService>();
            _originalProduct = product;
            
            PopulateForm();
            _logger?.Info($"EditProductDialog initialized for product: {product.Nume}");
        }

        private void PopulateForm()
        {
            NameTextBox.Text = _originalProduct.Nume;
            PriceNumericUpDown.Value = (decimal)_originalProduct.Pret;
            StockNumericUpDown.Value = (decimal)_originalProduct.Cantitate;
            
            // Set the category selection
            var categoryToSelect = _originalProduct.Tip.ToString();
            for (int i = 0; i < CategoryComboBox.Items.Count; i++)
            {
                if (CategoryComboBox.Items[i] is ComboBoxItem item && 
                    item.Tag?.ToString() == categoryToSelect)
                {
                    CategoryComboBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            WasUpdated = false;
            Close();
        }

        private async void UpdateProductAsync(object sender, RoutedEventArgs e)
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

                _logger?.Info($"Updating product: {name}, Price: {price}, Stock: {stock}, Category: {tipProdus}");

                // Update product using service
                var updatedProduct = await _service.ModificareProdus(_originalProduct.Id, name, tipProdus, price, stock);
                
                if (updatedProduct != null)
                {
                    WasUpdated = true;
                    ShowSuccess("Product updated successfully!");
                    _logger?.Info($"Product updated successfully: {name}");
                    
                    // Close dialog after short delay
                    await Task.Delay(1000);
                    Close();
                }
                else
                {
                    ShowError("Failed to update product. Please try again.");
                    _logger?.Error($"Failed to update product: {name}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error updating product: {ex.Message}");
                _logger?.Error("Error updating product", ex);
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