using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ogur.Terraria.Manager.Devexpress.ViewModels;
using Ogur.Terraria.Manager.Infrastructure.Services;

namespace Ogur.Terraria.Manager.Devexpress.Views;

public partial class LoginView : UserControl
{
    public LoginView(LoginViewModel vm, IMessenger messenger)
    {
        InitializeComponent();
        DataContext = vm;

        messenger.Register<LoginSucceededMessage>(this, (r, m) =>
        {
            // Login succeeded - AppFlowCoordinator will handle navigation
        });
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
        {
            vm.LoginCommand.Execute(null);
        }
    }
}