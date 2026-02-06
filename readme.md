# Ogur Terraria Manager

A desktop application for managing Terraria dedicated servers via SSH. Built with WPF, DevExpress, and .NET 8.0.

## Background

This project was born out of a practical need. My friends were playing on my Terraria server, and every time they needed to execute server commands, they had to SSH into the Linux host. Since most of them weren't comfortable with command-line interfaces, I built this GUI application to give them a simple, user-friendly way to manage the server.

## Features

### Authentication & Security
- Integration with Ogur.Hub authentication system
- Secure SSH connection management
- Encrypted password storage
- Auto-connect on startup option

### Server Management
- Real-time console output with ANSI color support
- Direct command execution via SSH
- Docker container attachment (supports multiple containers)
- Connection status indicator (color-coded: red/yellow/green)

### Command Interface
- **Console View**: Traditional terminal-style interface with command history
- **Commands View**: Visual button-based interface organized by categories:
    - Time Commands (dawn, noon, dusk, midnight)
    - Server Management (save, version, seed, etc.)
    - Admin Commands (kick, ban, server shutdown)
- Quick-access buttons for common operations
- Input dialogs for commands requiring parameters
- Confirmation prompts for destructive actions

### User Experience
- Dark theme UI optimized for extended use
- Tab-based navigation (Console, Commands, Settings)
- Keyboard shortcuts:
    - `Enter` - Send command
    - `Ctrl+Enter` - Send chat message (say command)
    - `Enter` - Execute in input dialogs
    - `Escape` - Cancel input dialogs
- Always-on-top window option
- Window dragging from anywhere
- Custom borderless window design

### Technical Features
- Multi-container support (easily switch between test/production servers)
- Tmux session management for persistent connections
- UTF-8 encoding without BOM for proper SSH compatibility
- ANSI escape code filtering for clean console output
- Automatic console output limiting (10,000 characters)
- Settings persistence in AppData

## Architecture
```
Ogur.Terraria.Manager/
├── Core/                          # Domain models
│   └── Models/
│       └── ServerCommand.cs       # Command definitions with input/confirmation flags
├── Infrastructure/                # Services and configuration
│   ├── Config/
│   │   └── AppSettings.cs        # Application settings with encryption
│   ├── Services/
│   │   ├── SshService.cs         # SSH/Docker management
│   │   ├── NavigationService.cs  # View navigation
│   │   └── AppFlowCoordinator.cs # Startup flow orchestration
│   └── Messages/
│       └── NavigationMessage.cs   # MVVM messaging
├── Devexpress/                    # UI Layer
│   ├── ViewModels/
│   │   ├── ShellViewModel.cs     # Main window
│   │   ├── ConsoleViewModel.cs   # Console logic
│   │   ├── SettingsViewModel.cs  # Settings management
│   │   └── LoginViewModel.cs     # Authentication
│   ├── Views/
│   │   ├── ShellWindow.xaml      # Main window
│   │   ├── MainView.xaml         # Tab container with status bar
│   │   ├── ConsoleView.xaml      # Terminal-style interface
│   │   ├── CommandsView.xaml     # Button-based interface
│   │   ├── SettingsView.xaml     # Configuration
│   │   ├── LoginView.xaml        # Authentication UI
│   │   └── InputDialog.xaml      # Custom input prompt
│   └── Converters/
│       ├── StatusToColorConverter.cs      # Connection status colors
│       └── InverseBooleanConverter.cs     # Boolean negation for bindings
└── Assets/
    └── ogur.ico                   # Application icon
```

### Design Patterns
- **MVVM**: Clean separation of concerns with ViewModels
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **MVVM Messaging**: CommunityToolkit.Mvvm for loosely-coupled communication
- **Repository Pattern**: Settings persistence with encryption
- **Strategy Pattern**: Command execution with input/confirmation strategies

## Prerequisites

- .NET 8.0 Runtime
- Windows 10/11
- SSH access to a Linux server running Terraria in Docker
- Tmux installed on the server
- Server must have Docker container with `stdin_open: true`

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

The application requires a tmux session to be created by the SSH user:
```bash
# Switch to your SSH user (e.g., 'terraria')
su - terraria

# Create tmux session attached to Docker container
tmux new-session -d -s terraria "docker attach terraria"

# Verify session exists
tmux ls

# The session should show: terraria: 1 windows (created ...)
```

**Important Notes:**
- The tmux session MUST be created by the same user the application SSHs as
- Each Docker container requires its own tmux session
- Sessions persist across application restarts
- To manually detach from tmux: `Ctrl+B`, then `D`

### 3. User Permissions

Ensure your SSH user has:
- Docker access (add to `docker` group)
- Tmux installed
- Read/write access to the Terraria directories
```bash
# Add user to docker group
sudo usermod -aG docker terraria

# Install tmux if not present
sudo apt install tmux
```

## Installation

1. Download the latest release from GitHub
2. Extract to your preferred location
3. Run `Ogur.Terraria.Manager.exe`
4. Login with your Ogur.Hub credentials
5. Configure SSH settings in the Settings tab

## Configuration

### Settings Tab

**SSH Connection:**
- Host: Server IP or hostname
- Port: SSH port (default 22)
- Username: SSH username (must match tmux session creator)
- Password: SSH password (encrypted)
- Container Name: Docker container name (e.g., "terraria")
- Auto-connect: Automatically connect on startup

**Ogur.Hub Authentication:**
- API URL: Authentication server URL

**User Interface:**
- Console Font Size: 8-24pt
- Show Timestamps: Prefix console output with timestamps
- Always on Top: Keep window above other applications

### Command Configuration

Commands are defined in `Core/Models/ServerCommand.cs`:
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

1. Navigate to the **Settings** tab
2. Enter your SSH credentials and container name
3. Click **Connect** or enable **Auto-connect** for future launches
4. Status indicator shows connection state:
    - Red: Disconnected
    - Yellow: Connecting...
    - Green: Connected

### Using the Console

**Console Tab:**
- Type commands directly in the input box
- Press `Enter` to execute
- Press `Ctrl+Enter` to send a chat message (say command)
- Click **Clear** to clear console output
- Command history scrolls automatically

**Commands Tab:**
- Click category buttons to execute common commands
- Commands requiring input will show a dialog
- Destructive commands (kick, ban, shutdown) require confirmation
- All buttons are disabled when not connected

### Multiple Servers

To manage multiple Terraria servers:

1. Create separate Docker containers with different names
2. Create tmux sessions for each: `tmux new-session -d -s <name> "docker attach <name>"`
3. Switch between them using the **Container Name** setting
4. Each container maintains its own console history

## Troubleshooting

### "No sessions" error

**Cause:** Tmux session was created by a different user than the SSH user.

**Solution:**
```bash
# Delete wrong session
tmux kill-session -t terraria

# SSH as the correct user
ssh terraria@your-server

# Create session as this user
tmux new-session -d -s terraria "docker attach terraria"
```

### Commands not executing

**Cause:** Docker container not started with `stdin_open: true`.

**Solution:**
1. Add `stdin_open: true` to docker-compose.yml
2. Recreate container: `docker-compose up -d --force-recreate`
3. Recreate tmux session

### Connection timeout

**Cause:** Firewall blocking SSH or incorrect credentials.

**Solution:**
1. Verify SSH access manually: `ssh user@host`
2. Check firewall rules: `sudo ufw status`
3. Verify container is running: `docker ps`

### Input dialog hidden behind window

**Cause:** Application set to "Always on Top" but dialogs don't inherit this.

**Solution:** This is fixed in the latest version. Update to the newest release.

## Technologies

- **Framework:** .NET 8.0
- **UI:** WPF with DevExpress 24.1
- **MVVM:** CommunityToolkit.Mvvm 8.3.2
- **SSH:** SSH.NET (Renci.SshNet) 2024.2.0
- **DI:** Microsoft.Extensions.DependencyInjection 9.0.0
- **Authentication:** Ogur.Core & Ogur.Hub integration

## Development

### Building from Source
```bash
# Clone repository
git clone https://github.com/yourusername/Ogur.Terraria.Manager.git
cd Ogur.Terraria.Manager

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project Ogur.Terraria.Manager
```

### Project Structure

- **Core**: Domain models and business logic
- **Infrastructure**: Services, configuration, and external integrations
- **Devexpress**: UI layer with ViewModels and Views
- Clean architecture with dependency injection throughout

## License

[Your License Here]

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and questions, please open an issue on GitHub.

## Acknowledgments

- Terraria Server Docker Image: [beardedio/terraria](https://hub.docker.com/r/beardedio/terraria)
- SSH.NET Library: [sshnet/SSH.NET](https://github.com/sshnet/SSH.NET)
- DevExpress WPF Components