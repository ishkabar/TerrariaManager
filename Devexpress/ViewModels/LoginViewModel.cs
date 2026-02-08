using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Ogur.Abstractions.Hub;
using Ogur.Terraria.Manager.Infrastructure.Services;
using Ogur.Terraria.Manager.Infrastructure.Config;

namespace Ogur.Terraria.Manager.Devexpress.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ILogger<LoginViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly ILicenseValidator _licenseValidator;
    private readonly IMessenger _messenger;
    private readonly LoginCredentials _credentials;
    private bool _isClearing;

    [ObservableProperty] private string? _username;
    [ObservableProperty] private string? _password;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _rememberMe;

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    private Brush _errorForeground = Brushes.Red;
    public Brush ErrorForeground
    {
        get => _errorForeground;
        set => SetProperty(ref _errorForeground, value, nameof(ErrorForeground));
    }

    public LoginViewModel(
        ILogger<LoginViewModel> logger,
        IAuthService authService,
        ILicenseValidator licenseValidator,
        IMessenger messenger)
    {
        _logger = logger;
        _authService = authService;
        _licenseValidator = licenseValidator;
        _messenger = messenger;

        // Load saved credentials
        _credentials = LoginCredentials.Load();
        if (_credentials.RememberMe)
        {
            Username = _credentials.Username;
            Password = _credentials.GetPassword();
            RememberMe = true;
        }
    }

    partial void OnUsernameChanged(string? value)
    {
        if (!_isClearing)
            ErrorMessage = null;
        LoginCommand.NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string? value)
    {
        if (!_isClearing)
            ErrorMessage = null;
        LoginCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password";
            ErrorForeground = Brushes.Red;
            return;
        }

        ErrorMessage = null;
        IsLoading = true;

        try
        {
            _logger.LogInformation("Attempting login for user: {Username}", Username);

            var result = await _authService.LoginAsync(Username, Password, ct);

            if (result.Success)
            {
                _logger.LogInformation("Login successful, validating license...");

                var licenseResult = await _licenseValidator.ValidateAsync(ct);

                if (!licenseResult.IsValid)
                {
                    ErrorMessage = $"License validation failed: {licenseResult.ErrorMessage}";
                    ErrorForeground = Brushes.Red;
                    _authService.Logout();
                    return;
                }

                _logger.LogInformation("License valid until: {ExpiresAt}", licenseResult.ExpiresAt);

                // Save credentials if RememberMe is checked
                if (RememberMe)
                {
                    _credentials.Username = Username;
                    _credentials.SetPassword(Password);
                    _credentials.RememberMe = true;
                    _credentials.Save();
                }
                else
                {
                    _credentials.Clear();
                }

                _messenger.Send(new LoginSucceededMessage(result.Username));
            }
            else
            {
                var errorMsg = result.ErrorMessage ?? "Login failed";

                if (errorMsg.StartsWith("{"))
                {
                    try
                    {
                        var json = System.Text.Json.JsonDocument.Parse(errorMsg);
                        if (json.RootElement.TryGetProperty("error", out var errorProp))
                        {
                            errorMsg = errorProp.GetString() ?? errorMsg;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse error JSON");
                    }
                }

                ErrorMessage = errorMsg;
                ErrorForeground = Brushes.Red;
                _logger.LogWarning("Login failed for user {Username}: {Error}", Username, errorMsg);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
            ErrorForeground = Brushes.Red;
            _logger.LogError(ex, "Login exception for user: {Username}", Username);
        }
        finally
        {
            IsLoading = false;
            _isClearing = true;
            if (!RememberMe)
                Password = string.Empty;
            _isClearing = false;
        }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !IsLoading;
}