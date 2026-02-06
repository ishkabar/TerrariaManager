using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ogur.Terraria.Manager.Devexpress.ViewModels;

namespace Ogur.Terraria.Manager.Devexpress.Views;

public partial class MainView : UserControl
{
    private readonly IServiceProvider _serviceProvider;
    private bool _initialized;

    public MainView(IServiceProvider serviceProvider, ConsoleViewModel consoleViewModel)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        // MainView używa ConsoleViewModel dla Status Bar
        DataContext = consoleViewModel;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var consoleView = _serviceProvider.GetRequiredService<ConsoleView>();
        ConsoleTab.Content = consoleView;
        _initialized = true;
    }

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initialized) return;

        if (MainTabControl.SelectedItem == CommandsTab && CommandsTab.Content == null)
        {
            var commandsView = _serviceProvider.GetRequiredService<CommandsView>();
            CommandsTab.Content = commandsView;
        }
        else if (MainTabControl.SelectedItem == SettingsTab && SettingsTab.Content == null)
        {
            var settingsView = _serviceProvider.GetRequiredService<SettingsView>();
            SettingsTab.Content = settingsView;
        }
    }
}