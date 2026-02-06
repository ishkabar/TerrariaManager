using System;

namespace Ogur.Terraria.Manager.Core.Models;

public class ServerStatus
{
    public bool IsConnected { get; set; }
    public bool IsDockerAttached { get; set; }
    public int PlayersOnline { get; set; }
    public string WorldName { get; set; }
    public DateTime LastUpdate { get; set; }
}
