using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ogur.Terraria.Manager.Core.Models;
using Ogur.Terraria.Manager.Infrastructure.Services;
using Ogur.Terraria.Manager.Infrastructure.Config;

namespace Ogur.Terraria.Manager.Devexpress.ViewModels;

public class ConsoleViewModel : ViewModelBase, IDisposable
{
    private readonly SshService _sshService;
    private readonly AppSettings _settings;

    private string _consoleOutput = "";
    private string _commandInput = "";
    private bool _isConnected;
    private bool _isConnecting;
    private string _statusText = "Disconnected";
    private Visibility _commandButtonsVisibility = Visibility.Collapsed;

    public ObservableCollection<ServerCommand> Commands { get; }

    public ConsoleViewModel(SshService sshService, AppSettings settings)
    {
        _sshService = sshService;
        _settings = settings;

        Commands = new ObservableCollection<ServerCommand>();
        foreach (var cmd in ServerCommand.GetDefaultCommands())
        {
            Commands.Add(cmd);
        }

        _sshService.OutputReceived += OnOutputReceived;
        _sshService.ConnectionStateChanged += OnConnectionStateChanged;

        // Commands
        ConnectCommand = new DelegateCommand(async () => await ConnectAsync(), () => !_isConnecting);
        DisconnectCommand = new DelegateCommand(Disconnect, () => _isConnected);
        SendCommand = new DelegateCommand(async () => await SendCommandAsync(), () => _isConnected);
        ExecuteServerCommand = new DelegateCommand<ServerCommand>(async cmd => await ExecuteCommand(cmd));
        ClearConsoleCommand = new DelegateCommand(ClearConsole);

        _ = TryAutoConnectAsync();
    }

    #region Properties

    public string ConsoleOutput
    {
        get => _consoleOutput;
        set => SetProperty(ref _consoleOutput, value, nameof(ConsoleOutput));
    }

    public string CommandInput
    {
        get => _commandInput;
        set => SetProperty(ref _commandInput, value, nameof(CommandInput));
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (SetProperty(ref _isConnected, value, nameof(IsConnected)))
            {
                (ConnectCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                (DisconnectCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                (SendCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        set
        {
            if (SetProperty(ref _isConnecting, value, nameof(IsConnecting)))
            {
                (ConnectCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value, nameof(StatusText));
    }

    public Visibility CommandButtonsVisibility
    {
        get => _commandButtonsVisibility;
        set => SetProperty(ref _commandButtonsVisibility, value, nameof(CommandButtonsVisibility));
    }

    #endregion

    #region Commands

    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SendCommand { get; }
    public ICommand ExecuteServerCommand { get; }
    public ICommand ClearConsoleCommand { get; }

    private async Task TryAutoConnectAsync()
    {
        if (!_settings.AutoConnect)
            return;

        if (!string.IsNullOrEmpty(_settings.SshHost) &&
            !string.IsNullOrEmpty(_settings.SshUsername) &&
            !string.IsNullOrEmpty(_settings.SshPasswordHash))
        {
            AppendOutput("🔄 Auto-connecting to server...\n");
            await ConnectAsync();
        }
    }

    private async Task ConnectAsync()
    {
        if (string.IsNullOrEmpty(_settings.SshHost) || 
            string.IsNullOrEmpty(_settings.SshUsername))
        {
            AppendOutput("❌ SSH settings not configured. Go to Settings.\n");
            return;
        }

        var password = _settings.GetSshPassword();
        if (string.IsNullOrEmpty(password))
        {
            AppendOutput("❌ SSH password not set. Go to Settings.\n");
            return;
        }

        IsConnecting = true;
        StatusText = "Connecting...";
        AppendOutput($"🔌 Connecting to {_settings.SshHost}:{_settings.SshPort}...\n");

        var connected = await _sshService.ConnectAsync(
            _settings.SshHost,
            _settings.SshPort,
            _settings.SshUsername,
            password
        );

        if (connected)
        {
            Console.WriteLine($"🔍 DEBUG: ContainerName = '{_settings.ContainerName}'");
            AppendOutput($"🐋 Attaching to Docker container '{_settings.ContainerName}'...\n");
            AppendOutput("✅ SSH connected!\n");


            var attached = await _sshService.AttachToDockerAsync(_settings.ContainerName);
            if (attached)
            {
                AppendOutput("✅ Attached to Terraria server!\n");
                StatusText = "Connected";
                CommandButtonsVisibility = Visibility.Visible;
            }
            else
            {
                AppendOutput("❌ Failed to attach to Docker container\n");
                StatusText = "Connection failed";
            }
        }
        else
        {
            AppendOutput("❌ SSH connection failed\n");
            StatusText = "Connection failed";
        }

        IsConnecting = false;
    }

    public void ReloadSettings()
    {
        // Reload settings from disk
        var newSettings = AppSettings.Load();

        Console.WriteLine($"🔄 RELOADING: Old ContainerName = '{_settings.ContainerName}'");
        Console.WriteLine($"🔄 RELOADING: New ContainerName = '{newSettings.ContainerName}'");


        // Update current settings object
        _settings.SshHost = newSettings.SshHost;
        _settings.SshPort = newSettings.SshPort;
        _settings.SshUsername = newSettings.SshUsername;
        _settings.SshPasswordHash = newSettings.SshPasswordHash;
        _settings.ContainerName = newSettings.ContainerName;
        _settings.ApiUrl = newSettings.ApiUrl;
        _settings.FontSize = newSettings.FontSize;
        _settings.ShowTimestamps = newSettings.ShowTimestamps;
        _settings.AlwaysOnTop = newSettings.AlwaysOnTop;
    }

    private void Disconnect()
    {
        _sshService.Disconnect();
        AppendOutput("\n🔌 Disconnected from server\n");
        StatusText = "Disconnected";
        CommandButtonsVisibility = Visibility.Collapsed;
    }

    private async Task SendCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(CommandInput))
            return;

        var command = CommandInput.Trim();
        AppendOutput($"> {command}\n");

        await _sshService.SendCommandAsync(command);
        CommandInput = "";
    }

    private async Task ExecuteCommand(ServerCommand? command)
    {
        if (command == null)
            return;

        AppendOutput($"> {command.Command}\n");
        await _sshService.SendCommandAsync(command.Command);
    }

    private void ClearConsole()
    {
        ConsoleOutput = "";
    }

    #endregion

    #region Event Handlers

    private void OnOutputReceived(object? sender, string output)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AppendOutput(output);
        });
    }

    private void OnConnectionStateChanged(object? sender, bool isConnected)
    {
        IsConnected = isConnected;
    }

    private void AppendOutput(string text)
    {
        ConsoleOutput += text;

        // Keep max 10000 chars
        if (ConsoleOutput.Length > 10000)
        {
            ConsoleOutput = ConsoleOutput.Substring(ConsoleOutput.Length - 10000);
        }
    }

    #endregion

    public void Dispose()
    {
        _sshService.OutputReceived -= OnOutputReceived;
        _sshService.ConnectionStateChanged -= OnConnectionStateChanged;
    }
}
