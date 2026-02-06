using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ogur.Terraria.Manager.Infrastructure.Config;
using Ogur.Terraria.Manager.Infrastructure.Services;
using Ogur.Terraria.Manager.Devexpress.Views;

namespace Ogur.Terraria.Manager.Devexpress.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly INavigationService _navigation;

    [ObservableProperty] private string _sshHost = "";
    [ObservableProperty] private int _sshPort;
    [ObservableProperty] private string _sshUsername = "";
    [ObservableProperty] private string _sshPassword = "";
    [ObservableProperty] private string _containerName = "";
    //[ObservableProperty] private string _dockerContainer = "";
    [ObservableProperty] private bool _autoConnect;
    [ObservableProperty] private string _apiUrl = "";
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private bool _showTimestamps;
    [ObservableProperty] private bool _alwaysOnTop;

    public SettingsViewModel(AppSettings settings, INavigationService navigation)
    {
        _settings = settings;
        _navigation = navigation;

        _sshHost = _settings.SshHost;
        _sshPort = _settings.SshPort;
        _sshUsername = _settings.SshUsername;
        _sshPassword = _settings.GetSshPassword() ?? "";
        _containerName = _settings.ContainerName;
        _apiUrl = _settings.ApiUrl;
        _fontSize = _settings.FontSize;
        _showTimestamps = _settings.ShowTimestamps;
        _alwaysOnTop = _settings.AlwaysOnTop;
        _autoConnect = _settings.AutoConnect;
    }

    [RelayCommand]
    private void Save()
    {
        _settings.SshHost = SshHost;
        _settings.SshPort = SshPort;
        _settings.SshUsername = SshUsername;

        if (!string.IsNullOrEmpty(SshPassword))
        {
            _settings.SetSshPassword(SshPassword);
        }

        _settings.ContainerName = ContainerName; // ← POPRAW TO
        _settings.ApiUrl = ApiUrl;
        _settings.FontSize = FontSize;
        _settings.ShowTimestamps = ShowTimestamps;
        _settings.AlwaysOnTop = AlwaysOnTop;
        _settings.AutoConnect = AutoConnect;

        _settings.Save();

        DevExpress.Xpf.Core.DXMessageBox.Show(
            "Settings saved successfully!",
            "Settings",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information
        );

        _navigation.NavigateToConsoleWithReload();
    }

    [RelayCommand]
    private void Back()
    {
        _navigation.NavigateTo<ConsoleView>();
    }
}