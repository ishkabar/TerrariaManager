using System.Windows.Controls;
using Ogur.Terraria.Manager.Devexpress.ViewModels;
using Ogur.Terraria.Manager.Infrastructure.Config;

namespace Ogur.Terraria.Manager.Devexpress.Views;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
