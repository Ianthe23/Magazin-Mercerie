using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using log4net;

namespace magazin_mercerie.Views.ClientViews;

public partial class MainWindow : Window
{
    private readonly ILog _logger = LogManager.GetLogger(typeof(MainWindow));
    
    public MainWindow()
    {
        try
        {
            _logger.Debug("MainWindow constructor called");
            Console.WriteLine("MainWindow constructor called");
            
            InitializeComponent();
            
            _logger.Debug("MainWindow initialized successfully");
            Console.WriteLine("MainWindow initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in MainWindow constructor: {ex.Message}");
            _logger.Error("Error in MainWindow constructor", ex);
            throw; // Re-throw to ensure the error isn't silently caught
        }
    }

    private void InitializeComponent()
    {
        try
        {
            _logger.Debug("InitializeComponent called");
            AvaloniaXamlLoader.Load(this);
            _logger.Debug("XAML loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR loading XAML: {ex.Message}");
            _logger.Error("Error loading XAML", ex);
            throw; // Re-throw to ensure the error isn't silently caught
        }
    }
    
    private void ToggleSplitView(object sender, RoutedEventArgs e)
    {
        try
        {
            var splitView = this.FindControl<SplitView>("MainSplitView");
            if (splitView != null)
            {
                splitView.IsPaneOpen = !splitView.IsPaneOpen;
                _logger.Debug($"SplitView pane toggled to {splitView.IsPaneOpen}");
            }
            else
            {
                _logger.Warn("Could not find MainSplitView control");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error toggling split view", ex);
        }
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _logger.Info("MainWindow opened");
        Console.WriteLine("MainWindow opened successfully");
    }
}
