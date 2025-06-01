using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using magazin_mercerie.ViewModels;
using magazin_mercerie.Views.LoginViews;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using log4net;
using magazin_mercerie.Service;
using System.Threading.Tasks;

namespace magazin_mercerie;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }
    private readonly ILog _logger = LogManager.GetLogger(typeof(App));

    public override void Initialize()
    {
        // Just load the XAML - logging is already set up in Program.cs
        _logger.Debug("App.Initialize() called");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _logger.Info("OnFrameworkInitializationCompleted() called");
        
        try
        {
            _logger.Info("Application starting up");
            
            // Set up Dependency Injection
            var services = new ServiceCollection();
            
            // Database Context
            _logger.Debug("Configuring database context");
            services.AddDbContext<AppDbContext>(options => 
                options.UseSqlite("Data Source=magazin-mercerie.db"));
                
            // Register Repositories
            _logger.Debug("Registering repositories");
            services.AddScoped<global::IClientRepository, global::ClientRepository>();
            services.AddScoped<global::IAngajatRepository, global::AngajatRepository>();
            services.AddScoped<global::IPatronRepository, global::PatronRepository>();
            services.AddScoped<global::IProdusRepository, global::ProdusRepository>();
            services.AddScoped<global::IComandaRepository, global::ComandaRepository>();
            
            // Register Password Service
            _logger.Debug("Registering password service");
            services.AddScoped<IPasswordService, PasswordService>();
            
            // Register Cart Service
            _logger.Debug("Registering cart service");
            services.AddSingleton<ICartService, CartService>();
            
            // Register User Session Service
            _logger.Debug("Registering user session service");
            services.AddSingleton<IUserSessionService, UserSessionService>();
            
            // Register Product Update Notification Service
            _logger.Debug("Registering product update notification service");
            services.AddSingleton<IProductUpdateNotificationService, ProductUpdateNotificationService>();
            
            // Register Service
            _logger.Debug("Registering service layer");
            services.AddScoped<global::IService, global::Service>();
            
            // Build the service provider
            ServiceProvider = services.BuildServiceProvider();
            _logger.Info("Service provider built successfully");
            
            // Ensure database exists and is up-to-date
            try
            {
                _logger.Debug("Initializing database");
                using (var scope = ServiceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                    _logger.Info("Database created successfully");
                }
            }
            catch (Exception dbEx)
            {
                _logger.Error("Database initialization error", dbEx);
                throw;
            }
            
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit
            DisableAvaloniaDataAnnotationValidation();
            _logger.Debug("Validation disabled successfully");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                try
                {
                    _logger.Debug("Creating LoginWindow");
                    desktop.MainWindow = new LoginWindow
                    {
                        DataContext = new LoginViewModel(),
                    };
                    _logger.Info("LoginWindow created successfully");
                    
                    // Set up application exit handler
                    desktop.Exit += (sender, args) =>
                    {
                        _logger.Info("Application shutting down");
                    };
                }
                catch (Exception winEx)
                {
                    _logger.Error("Window creation error", winEx);
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Fatal("Application startup error", ex);
            
            // Display error message
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var errorWindow = new Avalonia.Controls.Window
                {
                    Title = "Error",
                    Width = 500,
                    Height = 300,
                    Content = new Avalonia.Controls.TextBlock
                    {
                        Text = $"Application Error: {ex.Message}\n\nCheck log files for details.",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(20)
                    }
                };
                desktop.MainWindow = errorWindow;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}