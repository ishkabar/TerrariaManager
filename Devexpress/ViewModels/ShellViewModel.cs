using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Ogur.Terraria.Manager.Infrastructure.Services;

namespace Ogur.Terraria.Manager.Devexpress.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty] private FrameworkElement? _currentView;

    public ShellViewModel(IMessenger messenger)
    {
        messenger.Register<NavigationMessage>(this, (r, m) =>
        {
            CurrentView = m.View;
        });
    }
}