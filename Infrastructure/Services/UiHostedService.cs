using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ogur.Terraria.Manager.Infrastructure.Services;

public class UiHostedService : IHostedService
{
    private readonly IAppFlowCoordinator _flowCoordinator;
    private readonly ILogger<UiHostedService> _logger;

    public UiHostedService(
        IAppFlowCoordinator flowCoordinator,
        ILogger<UiHostedService> logger)
    {
        _flowCoordinator = flowCoordinator;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UI Hosted Service starting");

        // Give UI time to initialize
        await Task.Delay(500, cancellationToken);

        await _flowCoordinator.InitializeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UI Hosted Service stopping");
        return Task.CompletedTask;
    }
}