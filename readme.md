# Ogur Terraria Manager

A desktop application for managing Terraria dedicated servers via SSH. Built with WPF, DevExpress, and .NET 8.0.

## 🎮 Server Support

This application supports multiple Terraria server types across different branches:

- **`main` branch**: Vanilla Terraria (beardedio/terraria)
- **`tModLoader` branch**: tModLoader with mods support ⭐
- **`tshock` branch**: TShock server (planned)

**Switch branches to use the version matching your server type.** Setup instructions for each are below.

---

## Background

This project was born out of a practical need. My friends were playing on my Terraria server, and every time they needed to execute server commands, they had to SSH into the Linux host. Since most of them weren't comfortable with command-line interfaces, I built this GUI application to give them a simple, user-friendly way to manage the server.

## Features

### Authentication & Security
- Integration with Ogur.Hub authentication system
- Secure SSH connection management
- Encrypted password storage (separate credentials file)
- **Remember Me** checkbox for auto-login
- Auto-connect on startup option

### Server Management
- Real-time console output with ANSI color support
- Direct command execution via SSH
- Docker container attachment (supports multiple containers)
- Connection status indicator (color-coded: red/yellow/green)
- **tModLoader branch**: Screen session + named pipe for reliable command execution

### Command Interface
- **Console View**: Traditional terminal-style interface with command history
- **Commands View**: Visual button-based interface organized by categories:
    - Time Commands (dawn, noon, dusk, midnight)
    - Server Management (save, version, seed, modlist*, etc.)
    - **Mods Category*** (BossProgress, HEROsAdmin, msaudit, msauditclear)
    - Admin Commands (kick, ban, server shutdown)
- Quick-access buttons for common operations
- Input dialogs for commands requiring parameters
- Confirmation prompts for destructive actions

*\* tModLoader branch only*

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
- **main branch**: Tmux session management for persistent connections
- **tModLoader branch**: Screen session + named pipe for command execution
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
│   │   ├── AppSettings.cs        # Application settings with encryption
│   │   └── LoginCredentials.cs   # Separate login storage (tModLoader)
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
- **main branch**: Tmux installed on server
- **tModLoader branch**: Screen installed (auto-installed by start script)
- Server must have Docker container with `stdin_open: true`

---

## Server Setup - Vanilla Terraria (`main` branch)

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
# Switch to your SSH user (e.g., 'terraria')
su - terraria

# Create tmux session attached to Docker container
tmux new-session -d -s terraria "docker attach terraria"

# Verify session exists
tmux ls
# Should show: terraria: 1 windows (created ...)
```

**Important Notes:**
- The tmux session MUST be created by the same user the application SSHs as
- Each Docker container requires its own tmux session
- Sessions persist across application restarts
- To manually detach from tmux: `Ctrl+B`, then `D`

### 3. User Permissions
```bash
# Add user to docker group
sudo usermod -aG docker terraria

# Install tmux if not present
sudo apt install tmux
```

---

## Server Setup - tModLoader (`tModLoader` branch)

### 1. Docker Compose Configuration
```yaml
services:
  terraria:
    image: jacobsmile/tmodloader1.4:latest
    container_name: terraria-serwer
    restart: unless-stopped
    ports:
      - "7777:7777"
    environment:
      - TMOD_PASS=your_password
      - TMOD_MOTD=Your MOTD here
      - TMOD_MAXPLAYERS=8
      - TMOD_WORLDNAME=YourWorld
      - TMOD_WORLDSIZE=3
      - TMOD_DIFFICULTY=2
      - TMOD_AUTOCREATE=3
      - TMOD_WORLDSEED=
    volumes:
      - ./Worlds:/data/tModLoader/Worlds
      - ./Mods:/data/tModLoader/Mods
      - ./start-server.sh:/start-server.sh
    entrypoint: ["/start-server.sh"]
    stdin_open: true
    tty: true
```

### 2. Create start-server.sh
```bash
#!/bin/bash
apt-get update -qq && apt-get install -y -qq screen

mkfifo /tmp/terraria_input

screen -dmS terraria bash -c "tail -f /tmp/terraria_input | /terraria-server/LaunchUtils/ScriptCaller.sh -server -tmlsavedirectory '/data/tModLoader' -steamworkshopfolder '/data/steamMods/steamapps/workshop' -config '/terraria-server/serverconfig.txt'"

sleep 5
screen -ls

exec tail -f /dev/null
```

Make it executable:
```bash
chmod +x start-server.sh
```

### 3. Folder Structure
```
your-terraria-folder/
├── docker-compose.yml
├── start-server.sh
├── Worlds/
│   └── YourWorld.wld
└── Mods/
    ├── enabled.json
    ├── CalamityMod.tmod
    ├── InfernumMode.tmod
    └── ... (other mods)
```

### 4. enabled.json Example
```json
{
  "EnabledMods": [
    "CalamityMod",
    "InfernumMode",
    "MagicStorage",
    "HEROsMod"
  ]
}
```

### 5. tModLoader-Specific Features

**Commands:**
- `modlist` - Show loaded mods
- `BossProgress` - View boss progression
- `HEROsAdmin` - Get admin info (returns admin password)
- `msaudit` - View Magic Storage audit log
- `msauditclear` - Clear Magic Storage audit log

**HEROsMod Admin Setup:**
1. Run `HEROsAdmin` command to get admin password
2. Join the game
3. Create account: `/register username password`
4. Login: `/login username password`
5. Become admin: `/auth <password_from_HEROsAdmin>`

---

## Installation

1. Download the latest release from GitHub
2. **Switch to the branch matching your server type**
3. Extract to your preferred location
4. Run `Ogur.Terraria.Manager.exe`
5. Login with your Ogur.Hub credentials (check **Remember Me** for auto-login)
6. Configure SSH settings in the Settings tab

## Configuration

### Settings Tab

**SSH Connection:**
- Host: Server IP or hostname
- Port: SSH port (default 22)
- Username: SSH username
- Password: SSH password (encrypted)
- Container Name: Docker container name (e.g., "terraria" or "terraria-serwer")
- Auto-connect: Automatically connect on startup

**Ogur.Hub Authentication:**
- API URL: Authentication server URL
- Remember Me: Save login credentials securely

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
    RequiresInput = true,
    InputPrompt = "Enter player name:",
    RequiresConfirm = true,
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

To manage multiple Terraria servers, switch between them using the **Container Name** setting in Settings tab.

## Troubleshooting

### Vanilla Branch - "No sessions" error

**Cause:** Tmux session was created by a different user than the SSH user.

**Solution:**
```bash
tmux kill-session -t terraria
ssh terraria@your-server
tmux new-session -d -s terraria "docker attach terraria"
```

### tModLoader Branch - Commands not executing

**Cause:** Named pipe not working or screen session crashed.

**Solution:**
```bash
docker exec terraria-serwer screen -ls
docker exec terraria-serwer ls -la /tmp/terraria_input
docker-compose restart
```

### tModLoader Branch - Mods not loading

**Cause:** `enabled.json` missing or incorrect format.

**Solution:**
1. Check `/Mods/enabled.json` exists
2. Verify mod files are in `/Mods/` directory
3. Ensure `EnabledMods` array contains correct mod names

### Common Issues

**Connection timeout:**
1. Verify SSH access: `ssh user@host`
2. Check firewall: `sudo ufw status`
3. Verify container: `docker ps`

**Container not started with `stdin_open: true`:**
1. Add `stdin_open: true` to docker-compose.yml
2. Recreate: `docker-compose up -d --force-recreate`

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

# Switch to desired branch
git checkout main          # For vanilla Terraria
# OR
git checkout tModLoader    # For tModLoader

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project Ogur.Terraria.Manager
```

## License

[Your License Here]

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and questions, please open an issue on GitHub.

## Acknowledgments

- Vanilla Terraria Docker: [beardedio/terraria](https://hub.docker.com/r/beardedio/terraria)
- tModLoader Docker: [jacobsmile/tmodloader1.4](https://hub.docker.com/r/jacobsmile/tmodloader1.4)
- SSH.NET Library: [sshnet/SSH.NET](https://github.com/sshnet/SSH.NET)
- DevExpress WPF Components