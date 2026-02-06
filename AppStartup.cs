using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ogur.Core.DependencyInjection;
using Ogur.Core.Hub;
using Ogur.Terraria.Manager.Devexpress.ViewModels;
using Ogur.Terraria.Manager.Devexpress.Views;
using Ogur.Terraria.Manager.Infrastructure.Services;
using Ogur.Terraria.Manager.Infrastructure.Config;

namespace Ogur.Terraria.Manager;

public static class AppStartup
{
    public static void Configure(HostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        // Ogur.Core
        builder.Services.AddOgurCore(configuration);
        builder.Services.AddOgurHub(configuration);

        builder.Services.PostConfigure<HubOptions>(options =>
        {
            options.ApiKey = HubConstants.ApiKey;
            options.ApplicationName = HubConstants.ApplicationName;
            options.ApplicationVersion = HubConstants.ApplicationVersion;
        });

        // Infrastructure
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IAppFlowCoordinator, AppFlowCoordinator>();
        builder.Services.AddSingleton<SshService>();
        builder.Services.AddSingleton(AppSettings.Load());

        // Shell
        builder.Services.AddSingleton<ShellWindow>();
        builder.Services.AddSingleton<ShellViewModel>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ConsoleViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Views
        builder.Services.AddTransient<LoginView>();
        builder.Services.AddTransient<MainView>();
        builder.Services.AddTransient<ConsoleView>();
        builder.Services.AddTransient<CommandsView>();
        builder.Services.AddTransient<SettingsView>();
        builder.Services.AddTransient<UpdateRequiredView>();

        // Background
        builder.Services.AddHostedService<UiHostedService>();
    }
}