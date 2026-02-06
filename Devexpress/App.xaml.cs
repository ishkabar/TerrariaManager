using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DevExpress.Xpf.Core;
using Ogur.Terraria.Manager.Devexpress.Views;

namespace Ogur.Terraria.Manager.Devexpress;

public partial class App : Application
{
    private IHost? _host;

    public static bool DebugMode { get; private set; }

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    private static IHost BuildHost()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        AppStartup.Configure(builder);
        return builder.Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var args = e.Args.Select(a => a.ToLower()).ToArray();
        DebugMode = args.Contains("--console") ||
                   args.Contains("-console") ||
                   args.Contains("--debug") ||
                   args.Contains("-debug");

        if (DebugMode)
        {
            AllocConsole();
            Console.WriteLine("🚀 Terraria Manager Console (Debug Mode)");
            Console.WriteLine("📋 Command line arguments:");
            foreach (var arg in e.Args)
            {
                Console.WriteLine($"   - {arg}");
            }
            Console.WriteLine();
        }

        DispatcherUnhandledException += (s, args) =>
        {
            var ex = args.Exception;
            var errorMessage = $"ERROR: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";

            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner Exception:\n{ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }

            if (DebugMode)
            {
                Console.WriteLine(errorMessage);
            }

            DXMessageBox.Show(
                errorMessage,
                "Error Details",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            args.Handled = true;
        };

        // ApplicationThemeHelper.ApplicationThemeName = Theme.VS2019DarkName; // ← Zakomentowane bo szuka v25

        Trace.Listeners.Clear();
        Trace.Listeners.Add(new CustomTraceListener());

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            DXMessageBox.Show(
                ex?.Message ?? "Unknown error",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        };

        // ✅ Build Host
        Console.WriteLine("🏗️ Building host...");
        _host = BuildHost();

        // ✅ Get ShellWindow
        Console.WriteLine("📺 Getting ShellWindow...");
        var shell = _host.Services.GetRequiredService<ShellWindow>();
        MainWindow = shell;

        // ✅ Show Window
        Console.WriteLine("👁️ Showing window...");
        shell.Show();

        // ✅ Start Host (background services)
        Console.WriteLine("▶️ Starting host services...");
        await _host.StartAsync();

        Console.WriteLine("✅ Application started!");
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (DebugMode)
        {
            Console.WriteLine("👋 Application exiting...");
        }

        if (_host is not null)
        {
            try
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
            finally
            {
                _host.Dispose();
            }
        }

        if (DebugMode)
        {
            FreeConsole();
        }

        base.OnExit(e);
    }
}

public class CustomTraceListener : TraceListener
{
    public override void Write(string message) { }
    public override void WriteLine(string message) { }

    public override void Fail(string message, string detailMessage)
    {
        DXMessageBox.Show(
            $"{message}\n\n{detailMessage}",
            "DevExpress Assertion",
            MessageBoxButton.OK,
            MessageBoxImage.Warning
        );
    }
}