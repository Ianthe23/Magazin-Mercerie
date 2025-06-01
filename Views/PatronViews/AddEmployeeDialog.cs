using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace magazin_mercerie.Views.PatronViews
{
    public partial class AddEmployeeDialog : Window
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(AddEmployeeDialog));
        private readonly IService _service;
        public Angajat? CreatedEmployee { get; private set; }

        public AddEmployeeDialog()
        {
            InitializeComponent();
            _service = App.ServiceProvider?.GetService<IService>();
            _logger.Info("AddEmployeeDialog initialized");
        }

        private void CloseDialog(object? sender, RoutedEventArgs e)
        {
            _logger.Debug("AddEmployeeDialog closed by user");
            Close();
        }

        private async void CreateEmployee(object? sender, RoutedEventArgs e)
        {
            try
            {
                _logger.Debug("Create Employee button clicked");
                
                // Hide previous messages
                HideAllMessages();
                
                // Validate form
                if (!ValidateForm())
                {
                    _logger.Warn("Form validation failed");
                    return;
                }

                // Show loading
                ShowLoading(true);

                // Get form values
                var name = NameTextBox.Text?.Trim();
                var email = EmailTextBox.Text?.Trim();
                var username = UsernameTextBox.Text?.Trim();
                var password = PasswordTextBox.Text?.Trim();
                var phone = PhoneTextBox.Text?.Trim();
                var salary = (int)SalaryNumericUpDown.Value;

                _logger.Info($"Creating employee: {name} ({email})");

                // Create employee using service
                var newEmployee = await _service.AdaugareAngajat(name, email, username, password, phone, salary);

                if (newEmployee != null)
                {
                    _logger.Info($"Successfully created employee: {name}");
                    CreatedEmployee = newEmployee as Angajat;
                    
                    // Show success message
                    ShowSuccess("Employee created successfully!");
                    
                    // Wait a moment then close
                    await Task.Delay(1500);
                    Close();
                }
                else
                {
                    _logger.Warn($"Failed to create employee: {name}");
                    ShowError("Failed to create employee. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error creating employee", ex);
                ShowError($"Error creating employee: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // Clear previous errors
            ClearValidationErrors();

            // Validate Name
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowFieldError(NameError, "Name is required");
                isValid = false;
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowFieldError(EmailError, "Email is required");
                isValid = false;
            }
            else if (!IsValidEmail(EmailTextBox.Text))
            {
                ShowFieldError(EmailError, "Please enter a valid email address");
                isValid = false;
            }

            // Validate Username
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                ShowFieldError(UsernameError, "Username is required");
                isValid = false;
            }
            else if (UsernameTextBox.Text.Length < 3)
            {
                ShowFieldError(UsernameError, "Username must be at least 3 characters");
                isValid = false;
            }

            // Validate Password
            if (string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                ShowFieldError(PasswordError, "Password is required");
                isValid = false;
            }
            else if (PasswordTextBox.Text.Length < 6)
            {
                ShowFieldError(PasswordError, "Password must be at least 6 characters");
                isValid = false;
            }

            // Validate Phone
            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                ShowFieldError(PhoneError, "Phone number is required");
                isValid = false;
            }

            // Validate Salary
            if (SalaryNumericUpDown.Value <= 0)
            {
                ShowFieldError(SalaryError, "Salary must be greater than 0");
                isValid = false;
            }

            return isValid;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ShowFieldError(TextBlock errorLabel, string message)
        {
            errorLabel.Text = message;
            errorLabel.IsVisible = true;
        }

        private void ClearValidationErrors()
        {
            NameError.IsVisible = false;
            EmailError.IsVisible = false;
            UsernameError.IsVisible = false;
            PasswordError.IsVisible = false;
            PhoneError.IsVisible = false;
            SalaryError.IsVisible = false;
        }

        private void HideAllMessages()
        {
            ErrorPanel.IsVisible = false;
            SuccessPanel.IsVisible = false;
            ClearValidationErrors();
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