using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Ogur.Terraria.Manager.Infrastructure.Services;
using Ogur.Terraria.Manager.Infrastructure.Config;

namespace Ogur.Terraria.Manager.Devexpress.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty] private FrameworkElement? _currentView;
    private readonly AppSettings _settings;
    public ConsoleViewModel ConsoleViewModel { get; }
    public ShellViewModel(IMessenger messenger, ConsoleViewModel consoleViewModel, AppSettings settings)
    {
        ConsoleViewModel = consoleViewModel;
        _settings = settings;

        messenger.Register<NavigationMessage>(this, (r, m) =>
        {
            CurrentView = m.View;
        });
    }

    public bool AlwaysOnTop => _settings.AlwaysOnTop;
}