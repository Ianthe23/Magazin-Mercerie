using Avalonia;
using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace magazin_mercerie;

// Add console output test before any other code runs
static class ConsoleTest
{
    public static bool Executed { get; private set; } = false;
    
    static ConsoleTest()
    {
        Console.WriteLine("===============================");
        Console.WriteLine("CONSOLE OUTPUT TEST: If you see this, console output is working");
        Console.WriteLine("TERMINAL TEST: This should appear in the terminal when running dotnet run");
        Console.WriteLine("===============================");
        Executed = true;
    }
}

class Program
{
    private static readonly ILog log = LogManager.GetLogger(typeof(Program));
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Set up logging first, before anything else happens
            ConfigureLogging();
            
            // Log application startup
            log.Info("Starting application...");
            Console.WriteLine("Application starting...");
            
            // Start Avalonia UI
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CRITICAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            log.Fatal("Application failed to start", ex);
            throw;
        }
    }
    
    private static void ConfigureLogging()
    {
        try
        {
            // Get current directory and project root directory for debugging
            string currentDir = Directory.GetCurrentDirectory();
            Console.WriteLine($"Current directory: {currentDir}");
            
            // Set up logging directory - use project directory, not bin directory
            string logsPath = Path.Combine(currentDir, "Logs");
            Console.WriteLine($"Logs directory path: {logsPath}");
            
            // Ensure the Logs directory exists
            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
                Console.WriteLine($"Created Logs directory at: {logsPath}");
            }
            else
            {
                Console.WriteLine($"Logs directory already exists at: {logsPath}");
            }
            
            // Set log path for logging configuration to use
            GlobalContext.Properties["LogPath"] = logsPath;
            
            // Configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            string configFilePath = Path.Combine(currentDir, "log4net.config");
            
            Console.WriteLine($"Loading log4net configuration from: {configFilePath}");
            if (!File.Exists(configFilePath))
            {
                Console.WriteLine($"ERROR: log4net configuration file not found at: {configFilePath}");
                return;
            }
            
            XmlConfigurator.Configure(logRepository, new FileInfo(configFilePath));
            Console.WriteLine("log4net configured successfully");
            
            // Write a test log entry directly to a file to verify file access
            string testLogPath = Path.Combine(logsPath, "direct-test.log");
            File.AppendAllText(testLogPath, $"{DateTime.Now} - Direct file write test{Environment.NewLine}");
            Console.WriteLine($"Wrote test entry to: {testLogPath}");
            
            // Test logging to verify setup
            log.Debug("Debug message - testing log4net configuration");
            log.Info("Info message - testing log4net configuration");
            log.Warn("Warning message - testing log4net configuration");
            log.Error("Error message - testing log4net configuration");
            
            // Force flush of any buffered content
            LogManager.Flush(5000);
            
            // Log success
            log.Info("Logging initialized successfully");
            Console.WriteLine($"Check log files in: {logsPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring logging: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
