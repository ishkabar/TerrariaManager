# Ogur.Terraria.Manager

[![wakatime](https://wakatime.com/badge/user/c59759de-7539-43ec-bc20-5cd3aeca16e5/project/ffde0ffd-88fa-49ac-90c7-48539f4743ce.svg?style=flat-square)](https://wakatime.com/badge/user/c59759de-7539-43ec-bc20-5cd3aeca16e5/project/ffde0ffd-88fa-49ac-90c7-48539f4743ce)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-DevExpress-512BD4?style=flat-square)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=flat-square&logo=windows)
![SSH](https://img.shields.io/badge/SSH-Renci.SshNet-000000?style=flat-square)

Desktop application for managing Terraria dedicated servers via SSH. Built for friends who needed a GUI alternative to command-line server management.

## Background

Built out of practical need - my friends were playing on my Terraria server, but they weren't comfortable with SSH and command-line interfaces. This GUI gives them a simple, user-friendly way to manage the server without touching a terminal.

## Features

### Authentication & Security
- **Ogur.Hub Integration**: Centralized authentication system
- **License Validation**: Device fingerprinting (HWID + GUID)
- **Secure SSH**: Encrypted password storage with auto-connect
- **Update Enforcement**: Required version checking

### Server Management
- **Real-time Console**: ANSI color support with automatic scrolling
- **SSH Connection**: Direct command execution via Renci.SshNet
- **Docker Integration**: Tmux session attachment to containers
- **Status Indicator**: Color-coded connection state (red/yellow/green)
- **Multi-Container**: Switch between test/production servers

### Command Interface

**Console View:**
- Traditional terminal-style interface with command history
- Direct command input with Enter to execute
- `Ctrl+Enter` for chat messages (say command)
- Clear button to reset console output
- UTF-8 encoding without BOM for proper SSH compatibility

**Commands View:**
- Visual button-based interface organized by categories:
  - **Time**: dawn, noon, dusk, midnight
  - **Server**: save, version, seed, port, maxplayers
  - **Admin**: kick, ban, password, motd, shutdown
- Quick-access buttons for common operations
- Input dialogs for commands requiring parameters
- Confirmation prompts for destructive actions

### User Experience
- **Dark Theme**: Optimized for extended use
- **Tab Navigation**: Console, Commands, Settings
- **Keyboard Shortcuts**:
  - `Enter` - Send command
  - `Ctrl+Enter` - Send chat message
  - `Escape` - Cancel input dialogs
- **Always-on-Top**: Pin window above other applications
- **Drag Anywhere**: Move window from any point
- **Borderless Design**: Custom WPF window chrome

## Architecture
```
Ogur.Terraria.Manager/
├── Core/                           # Domain models
│   └── Models/
│       ├── ServerCommand.cs        # Command definitions with flags
│       └── ServerStatus.cs         # Connection state enum
├── Infrastructure/                 # Services
│   ├── Config/
│   │   └── AppSettings.cs         # Encrypted settings persistence
│   ├── Services/
│   │   ├── SshService.cs          # SSH/Tmux/Docker management
│   │   ├── ApiClient.cs           # Ogur.Hub communication
│   │   ├── NavigationService.cs   # View navigation
│   │   └── AppFlowCoordinator.cs  # Startup orchestration
│   └── Messages/
│       └── NavigationMessage.cs    # MVVM messaging
├── Devexpress/                     # UI Layer
│   ├── ViewModels/
│   │   ├── ShellViewModel.cs      # Main window logic
│   │   ├── ConsoleViewModel.cs    # Console + commands
│   │   ├── SettingsViewModel.cs   # Configuration
│   │   ├── LoginViewModel.cs      # Authentication
│   │   └── UpdateRequiredViewModel.cs  # Update enforcement
│   ├── Views/
│   │   ├── ShellWindow.xaml       # Main window
│   │   ├── MainView.xaml          # Tab container + status bar
│   │   ├── ConsoleView.xaml       # Terminal interface
│   │   ├── CommandsView.xaml      # Button interface
│   │   ├── SettingsView.xaml      # Settings form
│   │   ├── LoginView.xaml         # Auth UI
│   │   ├── InputDialog.xaml       # Custom input prompt
│   │   ├── UpdateRequiredView.xaml     # Update blocker
│   │   └── LicenseExpiredWindow.xaml   # License blocker
│   └── Converters/
│       ├── StatusToColorConverter.cs       # Connection colors
│       ├── InverseBoolConverter.cs         # Boolean negation
│       └── NullToCollapsedConverter.cs     # Visibility binding
└── Assets/
    └── ogur.ico                    # Application icon
```

### Design Patterns
- **MVVM**: Clean separation with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Repository Pattern**: AppSettings with encryption
- **Strategy Pattern**: Command execution with input/confirmation strategies
- **Messaging**: Loosely-coupled view navigation

## Tech Stack

- **.NET 8.0-windows** - WPF framework
- **DevExpress 24.1** - Professional UI components
- **SSH.NET** (Renci.SshNet 2024.2.0) - SSH client library
- **CommunityToolkit.Mvvm** (8.3.2) - MVVM helpers
- **Ogur.Hub Integration** - Authentication and licensing

## Prerequisites

- .NET 8.0 Runtime
- Windows 10/11
- SSH access to Linux server running Terraria in Docker
- Tmux installed on server
- Docker container with `stdin_open: true`

## Server Setup

### 1. Docker Compose Configuration
```yaml
services:
  terraria:
    image: beardedio/terraria:latest
    container_name: terraria
    restart: unless-stopped
    ports:
      - "7777:7777"
    volumes:
      - ./world:/root/.local/share/Terraria/Worlds
      - ./config:/config
    stdin_open: true  # CRITICAL: Required for console input
    tty: false
```

### 2. Tmux Session Setup
```bash
# Switch to SSH user (must match application SSH credentials)
su - terraria

# Create tmux session attached to Docker container
tmux new-session -d -s terraria "docker attach terraria"

# Verify session exists
tmux ls
# Output: terraria: 1 windows (created ...)
```

**Important:**
- Tmux session MUST be created by the same user the app SSHs as
- Each Docker container requires its own tmux session
- Sessions persist across application restarts
- Detach manually: `Ctrl+B`, then `D`

### 3. User Permissions
```bash
# Add user to docker group
sudo usermod -aG docker terraria

# Install tmux if not present
sudo apt install tmux

# Verify Docker access
docker ps
```

## Installation

1. Download latest release from GitHub
2. Extract to preferred location
3. Run `Ogur.Terraria.Manager.exe`
4. Login with Ogur.Hub credentials
5. Configure SSH settings in Settings tab

## Configuration

### Settings Tab

**SSH Connection:**
- Host: Server IP or hostname
- Port: SSH port (default 22)
- Username: SSH username (must match tmux session creator)
- Password: SSH password (encrypted with DPAPI)
- Container Name: Docker container name (e.g., "terraria")
- Auto-connect: Automatically connect on startup

**Ogur.Hub Authentication:**
- API URL: Authentication server URL (https://api.hub.ogur.dev)

**User Interface:**
- Console Font Size: 8-24pt
- Show Timestamps: Prefix console output with timestamps
- Always on Top: Keep window above other applications

### Command Configuration

Commands defined in `Core/Models/ServerCommand.cs`:
```csharp
new ServerCommand 
{ 
    Name = "Kick", 
    Command = "kick", 
    Icon = "🚫", 
    Category = "Admin",
    RequiresInput = true,           // Shows input dialog
    InputPrompt = "Enter player name:",
    RequiresConfirm = true,         // Shows confirmation dialog
    ConfirmMessage = "Kick this player?"
}
```

## Usage

### Connecting to Server

1. Navigate to **Settings** tab
2. Enter SSH credentials and container name
3. Click **Connect** or enable **Auto-connect**
4. Status indicator shows state:
   - 🔴 Red: Disconnected
   - 🟡 Yellow: Connecting...
   - 🟢 Green: Connected

### Using the Console

**Console Tab:**
- Type commands directly in input box
- Press `Enter` to execute
- Press `Ctrl+Enter` to send chat message (say command)
- Click **Clear** to reset console output
- Command history scrolls automatically
- ANSI color codes filtered for clean output
- Auto-limits to 10,000 characters

**Commands Tab:**
- Click category buttons to execute common commands
- Input dialogs appear for commands requiring parameters
- Confirmation prompts for destructive actions (kick, ban, shutdown)
- All buttons disabled when disconnected

### Multiple Servers

To manage multiple Terraria servers:

1. Create separate Docker containers with different names
2. Create tmux sessions for each:
```bash
   tmux new-session -d -s terraria-test "docker attach terraria-test"
   tmux new-session -d -s terraria-prod "docker attach terraria-prod"
```
3. Switch between them using **Container Name** setting
4. Each container maintains independent console history

## Troubleshooting

### "No sessions" error

**Cause:** Tmux session created by different user than SSH user.

**Solution:**
```bash
# Delete wrong session
tmux kill-session -t terraria

# SSH as correct user
ssh terraria@your-server

# Create session as this user
tmux new-session -d -s terraria "docker attach terraria"
```

### Commands not executing

**Cause:** Docker container not started with `stdin_open: true`.

**Solution:**
1. Add `stdin_open: true` to `docker-compose.yml`
2. Recreate container: `docker-compose up -d --force-recreate`
3. Recreate tmux session

### Connection timeout

**Cause:** Firewall blocking SSH or incorrect credentials.

**Solution:**
1. Verify SSH access manually: `ssh user@host`
2. Check firewall rules: `sudo ufw status`
3. Verify container is running: `docker ps`
4. Test tmux session: `tmux ls`

### License validation failed

**Cause:** Invalid license, device limit exceeded, or Hub connection issues.

**Solution:**
1. Verify Ogur.Hub is accessible
2. Check license status in Hub admin panel
3. Ensure device count is within limit (1 license = 2 devices max)
4. Contact administrator if license expired

### Input dialog hidden

**Cause:** Application set to "Always on Top" but dialogs don't inherit.

**Solution:** Fixed in latest version - dialogs now properly inherit Topmost property.

## Command Reference

### Time Commands
- `dawn` - Set time to 4:30 AM
- `noon` - Set time to 12:00 PM
- `dusk` - Set time to 7:30 PM
- `midnight` - Set time to 12:00 AM

### Server Commands
- `save` - Force world save
- `version` - Display server version
- `seed` - Show world seed
- `port` - Show server port
- `maxplayers` - Show max player count

### Admin Commands
- `kick <player>` - Kick player from server
- `ban <player>` - Ban player by name
- `password <pwd>` - Set server password
- `motd <text>` - Set message of the day
- `exit` - Shutdown server (with confirmation)

## Development

### Building from Source
```bash
# Clone repository
git clone https://github.com/ishkabar/Ogur.Terraria.Manager.git
cd Ogur.Terraria.Manager

# Restore dependencies
dotnet restore

# Build
dotnet build -c Release

# Run
dotnet run
```

### Project Structure

- **Core**: Domain models (ServerCommand, ServerStatus)
- **Infrastructure**: Services (SSH, Hub, Navigation, Settings)
- **Devexpress**: UI layer (ViewModels, Views, Converters)
- Clean architecture with DI throughout

### Dependencies
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
<PackageReference Include="DevExpress.Wpf.Themes.Office2019Colorful" Version="24.1.6" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
<PackageReference Include="Renci.SshNet" Version="2024.2.0" />
```

## License

Proprietary - All rights reserved © 2025 Dominik Karczewski (ogur.dev)

## Acknowledgments

- **Terraria Docker Image**: [beardedio/terraria](https://hub.docker.com/r/beardedio/terraria)
- **SSH.NET Library**: [sshnet/SSH.NET](https://github.com/sshnet/SSH.NET)
- **DevExpress WPF**: Professional UI components

## Author

**Dominik Karczewski**
- Website: [ogur.dev](https://ogur.dev)
- GitHub: [@ishkabar](https://github.com/ishkabar)
