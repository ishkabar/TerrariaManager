using System.Collections.Generic;

namespace Ogur.Terraria.Manager.Core.Models;

public class ServerCommand
{
    public string Name { get; set; }
    public string Command { get; set; }
    public string Icon { get; set; }
    public string Category { get; set; }

    public static List<ServerCommand> GetDefaultCommands()
    {
        return new List<ServerCommand>
        {
            // Time commands
            new ServerCommand { Name = "Świt", Command = "dawn", Icon = "🌅", Category = "Time" },
            new ServerCommand { Name = "Południe", Command = "noon", Icon = "☀️", Category = "Time" },
            new ServerCommand { Name = "Zmierzch", Command = "dusk", Icon = "🌆", Category = "Time" },
            new ServerCommand { Name = "Północ", Command = "midnight", Icon = "🌙", Category = "Time" },

            // Server management
            new ServerCommand { Name = "Save", Command = "save", Icon = "💾", Category = "Server" },
            new ServerCommand { Name = "Gracze", Command = "playing", Icon = "👥", Category = "Server" },
            new ServerCommand { Name = "Czas", Command = "time", Icon = "⏰", Category = "Server" },
            new ServerCommand { Name = "Settle Water", Command = "settle", Icon = "💧", Category = "Server" },
            new ServerCommand { Name = "Version", Command = "version", Icon = "ℹ️", Category = "Server" },
            new ServerCommand { Name = "Seed", Command = "seed", Icon = "🌱", Category = "Server" },
            new ServerCommand { Name = "MOTD", Command = "motd", Icon = "📢", Category = "Server" },
        };
    }
}
