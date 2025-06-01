using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace magazin_mercerie.Views.PatronViews
{
    public partial class EditEmployeeDialog : Window
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(EditEmployeeDialog));
        private readonly IService _service;
        private readonly Angajat _employee;
        public bool WasUpdated { get; private set; }

        public EditEmployeeDialog(Angajat employee)
        {
            InitializeComponent();
            _service = App.ServiceProvider?.GetService<IService>();
            _employee = employee ?? throw new ArgumentNullException(nameof(employee));
            _logger.Info($"EditEmployeeDialog initialized for employee: {employee.Nume}");
            
            // Populate form with existing data
            PopulateForm();
        }

        private void PopulateForm()
        {
            NameTextBox.Text = _employee.Nume;
            EmailTextBox.Text = _employee.Email;
            UsernameTextBox.Text = _employee.Username;
            PhoneTextBox.Text = _employee.Telefon;
            SalaryNumericUpDown.Value = _employee.Salariu;
            // Note: Password field is left blank for security
        }

        private void CloseDialog(object? sender, RoutedEventArgs e)
        {
            _logger.Debug("EditEmployeeDialog closed by user");
            Close();
        }

        private async void UpdateEmployee(object? sender, RoutedEventArgs e)
        {
            try
            {
                _logger.Debug($"Update Employee button clicked for: {_employee.Nume}");
                
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
                var phone = PhoneTextBox.Text?.Trim();
                var salary = (int)SalaryNumericUpDown.Value;
                
                // Use existing password if no new password is provided
                var password = string.IsNullOrWhiteSpace(PasswordTextBox.Text) 
                    ? _employee.Password  // Keep current password (already hashed)
                    : PasswordTextBox.Text.Trim();  // This will be hashed by the service

                _logger.Info($"Updating employee: {name} ({email})");

                // Update employee using service
                var updatedEmployee = await _service.ModificareAngajat(_employee.Id, name, email, username, password, phone, salary);

                if (updatedEmployee != null)
                {
                    _logger.Info($"Successfully updated employee: {name}");
                    WasUpdated = true;
                    
                    // Show success message
                    ShowSuccess("Employee updated successfully!");
                    
                    // Wait a moment then close
                    await Task.Delay(1500);
                    Close();
                }
                else
                {
                    _logger.Warn($"Failed to update employee: {name}");
                    ShowError("Failed to update employee. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating employee {_employee.Nume}", ex);
                ShowError($"Error updating employee: {ex.Message}");
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

            // Validate Password (only if provided)
            if (!string.IsNullOrWhiteSpace(PasswordTextBox.Text) && PasswordTextBox.Text.Length < 6)
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