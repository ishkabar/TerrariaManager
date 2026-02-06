using System;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Ogur.Terraria.Manager.Devexpress.Views;
using Ogur.Terraria.Manager.Devexpress.ViewModels;

namespace Ogur.Terraria.Manager.Infrastructure.Services;

public interface INavigationService
{
    void NavigateTo<TView>() where TView : FrameworkElement;
    void NavigateToConsoleWithReload();
}

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessenger _messenger;

    public NavigationService(IServiceProvider serviceProvider, IMessenger messenger)
    {
        _serviceProvider = serviceProvider;
        _messenger = messenger;
    }

    public void NavigateTo<TView>() where TView : FrameworkElement
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var view = _serviceProvider.GetRequiredService<TView>();
            _messenger.Send(new NavigationMessage { View = view });
        });
    }

    public void NavigateToConsoleWithReload()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Reload settings first
            var consoleView = _serviceProvider.GetRequiredService<ConsoleView>();
            if (consoleView.DataContext is ConsoleViewModel vm)
            {
                vm.ReloadSettings();
            }

            // Navigate to MainView (with tabs)
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            _messenger.Send(new NavigationMessage { View = mainView });
        });
    }
}

public class NavigationMessage
{
    public FrameworkElement? View { get; set; }
}