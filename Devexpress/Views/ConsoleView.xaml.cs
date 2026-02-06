using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ogur.Terraria.Manager.Devexpress.ViewModels;
using Ogur.Terraria.Manager.Infrastructure.Services;

namespace Ogur.Terraria.Manager.Devexpress.Views;

public partial class ConsoleView : UserControl
{
    private readonly INavigationService _navigation;

    public ConsoleView(ConsoleViewModel viewModel, INavigationService navigation)
    {
        InitializeComponent();
        DataContext = viewModel;
        _navigation = navigation;
    }

    private void CommandInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is ConsoleViewModel vm && vm.SendCommand.CanExecute(null))
        {
            vm.SendCommand.Execute(null);
        }
    }

    private void ConsoleOutput_TextChanged(object sender, TextChangedEventArgs e)
    {
        ConsoleScrollViewer.ScrollToEnd();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        _navigation.NavigateTo<SettingsView>();
    }
}