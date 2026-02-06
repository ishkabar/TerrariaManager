using System.Collections.Generic;

namespace Ogur.Terraria.Manager.Core.Models;

public class ServerCommand
{
    public string Name { get; set; }
    public string Command { get; set; }
    public string Icon { get; set; }
    public string Category { get; set; }
    public bool RequiresInput { get; set; }
    public string InputPrompt { get; set; } = "Enter value:";
    public bool RequiresConfirm { get; set; }
    public string ConfirmMessage { get; set; } = "Are you sure?";

    public static List<ServerCommand> GetDefaultCommands()
    {
        return new List<ServerCommand>
        {
            // Time commands
            new ServerCommand { Name = "Dawn", Command = "dawn", Icon = "🌅", Category = "Time" },
            new ServerCommand { Name = "Noon", Command = "noon", Icon = "☀️", Category = "Time" },
            new ServerCommand { Name = "Dusk", Command = "dusk", Icon = "🌆", Category = "Time" },
            new ServerCommand { Name = "Midnight", Command = "midnight", Icon = "🌙", Category = "Time" },

            // Server management
            new ServerCommand { Name = "Save", Command = "save", Icon = "💾", Category = "Server" },
            new ServerCommand { Name = "Playing", Command = "playing", Icon = "👥", Category = "Server" },
            new ServerCommand { Name = "Time", Command = "time", Icon = "⏰", Category = "Server" },
            new ServerCommand { Name = "Settle", Command = "settle", Icon = "💧", Category = "Server" },
            new ServerCommand { Name = "Version", Command = "version", Icon = "ℹ️", Category = "Server" },
            new ServerCommand { Name = "Seed", Command = "seed", Icon = "🌱", Category = "Server" },
            new ServerCommand { Name = "MOTD", Command = "motd", Icon = "📢", Category = "Server" },
            new ServerCommand { Name = "Change MOTD", Command = "motd", Icon = "✏️", Category = "Server", RequiresInput = true, InputPrompt = "Enter MOTD:", RequiresConfirm = true, ConfirmMessage = "Change server MOTD?" },
            new ServerCommand { Name = "Help", Command = "help", Icon = "❓", Category = "Server" },
            new ServerCommand { Name = "Clear", Command = "clear", Icon = "🗑️", Category = "Server" },
            new ServerCommand { Name = "Port", Command = "port", Icon = "🔌", Category = "Server" },
            new ServerCommand { Name = "Max Players", Command = "maxplayers", Icon = "👤", Category = "Server" },
            new ServerCommand { Name = "Password", Command = "password", Icon = "🔒", Category = "Server", RequiresInput = true, InputPrompt = "Enter new password:", RequiresConfirm = true, ConfirmMessage = "Change server password?" },

            // Admin commands
            new ServerCommand { Name = "Kick", Command = "kick", Icon = "🚫", Category = "Admin", RequiresInput = true, InputPrompt = "Enter player name:", RequiresConfirm = true, ConfirmMessage = "Kick this player?" },
            new ServerCommand { Name = "Ban", Command = "ban", Icon = "🔨", Category = "Admin", RequiresInput = true, InputPrompt = "Enter player name:", RequiresConfirm = true, ConfirmMessage = "Ban this player?" },
            new ServerCommand { Name = "Exit", Command = "exit", Icon = "💾➡️", Category = "Admin", RequiresConfirm = true, ConfirmMessage = "Shutdown server and save?" },
            new ServerCommand { Name = "Exit No Save", Command = "exit-nosave", Icon = "⚠️➡️", Category = "Admin", RequiresConfirm = true, ConfirmMessage = "Shutdown server WITHOUT saving?" },
        };
    }
}