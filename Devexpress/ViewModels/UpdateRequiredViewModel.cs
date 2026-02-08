// File: Ogur.Fishing.Host.Wpf/ViewModels/UpdateRequiredViewModel.cs

using System;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ogur.Terraria.Manager.Devexpress.ViewModels;

/// <summary>
/// ViewModel for the update required view.
/// </summary>
public sealed partial class UpdateRequiredViewModel : ObservableObject
{
    private readonly ILogger<UpdateRequiredViewModel> _logger;

    [ObservableProperty]
    private string _currentVersion = string.Empty;

    [ObservableProperty]
    private string _latestVersion = string.Empty;

    [ObservableProperty]
    private string? _releaseNotes;

    [ObservableProperty]
    private string _downloadUrl = string.Empty;

    public bool HasReleaseNotes => !string.IsNullOrWhiteSpace(ReleaseNotes);

    public UpdateRequiredViewModel(ILogger<UpdateRequiredViewModel> logger)
    {
        _logger = logger;
    }

    public void Initialize(string currentVersion, string latestVersion, string downloadUrl, string? releaseNotes)
    {
        _logger.LogInformation("🔥 Initialize called: Current={Current}, Latest={Latest}", currentVersion, latestVersion);

        
        CurrentVersion = currentVersion;
        LatestVersion = latestVersion;
        DownloadUrl = downloadUrl;
        ReleaseNotes = releaseNotes;
        
        // ✅ Notify that HasReleaseNotes might have changed
        OnPropertyChanged(nameof(HasReleaseNotes));
        _logger.LogInformation("🔥 After set: CurrentVersion={Current}, LatestVersion={Latest}", CurrentVersion, LatestVersion);

    }

    // ✅ Notify HasReleaseNotes when ReleaseNotes changes
    partial void OnReleaseNotesChanged(string? value)
    {
        OnPropertyChanged(nameof(HasReleaseNotes));
    }

    [RelayCommand]
    private void Download()
    {
        try
        {
            _logger.LogInformation("Opening download URL: {Url}", DownloadUrl);
            Process.Start(new ProcessStartInfo
            {
                FileName = DownloadUrl,
                UseShellExecute = true
            });

            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open download URL");
            MessageBox.Show(
                $"Failed to open download URL. Please visit:\n{DownloadUrl}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Exit()
    {
        _logger.LogInformation("User chose to exit without updating");
        Application.Current.Shutdown();
    }
}