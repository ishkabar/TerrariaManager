using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Ogur.Abstractions.Hub;
using Ogur.Terraria.Manager.Devexpress.Views;

namespace Ogur.Terraria.Manager.Infrastructure.Services;

public interface IAppFlowCoordinator
{
    Task InitializeAsync(CancellationToken ct);
}

public class AppFlowCoordinator : IAppFlowCoordinator
{
    private readonly INavigationService _navigation;
    private readonly IMessenger _messenger;
    private readonly IUpdateChecker _updateChecker;
    private readonly ILogger<AppFlowCoordinator> _logger;

    public AppFlowCoordinator(
        INavigationService navigation,
        IMessenger messenger,
        IUpdateChecker updateChecker,
        ILogger<AppFlowCoordinator> logger)
    {
        _navigation = navigation;
        _messenger = messenger;
        _updateChecker = updateChecker;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct)
    {
        _logger.LogInformation("Initializing app flow");

        // Check for required updates
        var updateResult = await _updateChecker.CheckForUpdatesAsync(
            HubConstants.ApplicationVersion, ct);

        if (updateResult.IsUpdateAvailable && updateResult.IsRequired)
        {
            _logger.LogCritical("Required update available: {Current} -> {Latest}",
                updateResult.CurrentVersion, updateResult.LatestVersion);

            _navigation.NavigateTo<UpdateRequiredView>();
            return;
        }

        // Register for login success
        _messenger.Register<LoginSucceededMessage>(this, (r, m) =>
        {
            _logger.LogInformation("Login succeeded for {Username}", m.Username);
            _navigation.NavigateTo<ConsoleView>();
        });

        // Navigate to login
        _navigation.NavigateTo<LoginView>();
    }
}

public record LoginSucceededMessage(string Username);