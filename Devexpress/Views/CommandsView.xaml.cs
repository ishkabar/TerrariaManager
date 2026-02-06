using System.Windows;
using System.Windows.Controls;
using Ogur.Terraria.Manager.Infrastructure.Services;

namespace Ogur.Terraria.Manager.Devexpress.Views;

public partial class CommandsView : UserControl
{
    private readonly INavigationService _navigation;

    public CommandsView(INavigationService navigation)
    {
        InitializeComponent();
        _navigation = navigation;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        _navigation.NavigateTo<SettingsView>();
    }
}