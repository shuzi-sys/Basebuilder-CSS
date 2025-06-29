using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Drawing;
using System.Data.SQLite;
using Dapper;


namespace Plugintest;

public partial class Plugintest : BasePlugin
{
    private List<CCSPlayerController> jugadores = new List<CCSPlayerController>();
    public List<Color> colors = new List<Color>() { Color.AliceBlue, Color.Aqua, Color.Blue, Color.Brown, Color.BurlyWood, Color.Chocolate, Color.Cyan, Color.DarkBlue, Color.DarkGreen, Color.DarkMagenta, Color.DarkOrange, Color.DarkRed, Color.Green, Color.Yellow, Color.Red, Color.Pink, Color.Purple, Color.ForestGreen, Color.LightCyan, Color.Lime };
    public Dictionary<CBaseProp, constructor> claimedblocks = new();
    public Dictionary<CCSPlayerController, CBaseProp> holdinglist = new();
    public Dictionary<CCSPlayerController, Color> playercolor = new();
    public Dictionary<CCSPlayerController, bool> wasPressingMap = new();
    public Dictionary<CCSPlayerController, int> playerlevel = new();
    public Dictionary<CCSPlayerController, int> playerexp = new();

    public override string ModuleName => "Plugintest";

    public override string ModuleVersion => "0.0.1";

    public override void Load(bool hotReload)
    {
        AddCommand("css_online", "definido", (player, commandInfo) =>
        {
            int conectados = jugadores.Count;
            commandInfo.ReplyToCommand(conectados.ToString());
        });

        AddCommand("css_lockobj", "definido", (player, commandInfo) =>
        {
            if (player.IsValid && player.PawnIsAlive)
            
            {
                var block = player.GetClientAimTarget();
                // Checking if already bonded
                if (claimedblocks.ContainsKey(block))
                {
                    // Checking ownership
                    if (claimedblocks[block].getowner() == player)
                    {
                        commandInfo.ReplyToCommand("ERROR: este objeto ya te pertenece");
                    }
                    else
                    {
                        commandInfo.ReplyToCommand("ERROR: este objeto ya fue reclamado");
                    }
                }
                else
                {
                    float relleno = 0;
                    var claim = new constructor(player, relleno);
                    claimedblocks.Add(block, claim);
                    commandInfo.ReplyToCommand("objeto bloqueado con exito");
                }
            }
            else
            {
                commandInfo.ReplyToCommand("ERROR: referencia a player no valida");
            }
        });

        AddCommand("css_refrescar", "definido", (player, commandInfo) =>
        {
            jugadores = Utilities.GetPlayers();
            commandInfo.ReplyToCommand("refrescado con exito");
        });

        AddCommand("css_lock", "definido", (player, commandInfo) =>
        {
            var block = TraceObject(new CounterStrikeSharp.API.Modules.Utils.Vector(player.PlayerPawn.Value!.AbsOrigin!.X, player.PlayerPawn.Value!.AbsOrigin!.Y, player.PlayerPawn.Value!.AbsOrigin!.Z + player.PlayerPawn.Value.CameraServices!.OldPlayerViewOffsetZ), player.PlayerPawn.Value!.EyeAngles!, false, true);
            if (block != null && block.IsValid)
            {
                if (claimedblocks.ContainsKey(block))
                {
                    var owner = claimedblocks[block].getowner().PlayerName;
                    commandInfo.ReplyToCommand("DEBUG: Este objeto le pertenece a " + owner);
                    player.PrintToCenter("Este objeto ya tiene dueño.");
                }
                else
                {
                    commandInfo.ReplyToCommand("DEBUG: Este objeto no fue claimeado");
                    int distance = 0;
                    var claim = new constructor(player, distance);
                    claimedblocks.Add(block, claim);
                    var color = colorcheck(player, block);
                    block.Render = color;
                    Server.NextWorldUpdate(() =>
                    {
                        Utilities.SetStateChanged(block, "CBaseProp", "m_clrRender");
                    });
                    player.PrintToCenter("Objeto reclamado con exito.");
                }
            }
            else
            {
                commandInfo.ReplyToCommand("DEBUG: Bloque no encontrado");
            }
        });

        AddCommand("css_lockcheck", "definido", (player, commandInfo) =>
        {
            var block = player.GetClientAimTarget();
            if (block != null && block.IsValid)
            {
                if (claimedblocks.ContainsKey(block))
                {
                    var owner = claimedblocks[block].getowner().PlayerName;
                    commandInfo.ReplyToCommand("DEBUG: Este objeto le pertenece a " + owner);
                }
                else
                {
                    commandInfo.ReplyToCommand("DEBUG: Este objeto no fue claimeado");
                }
            }
            else
            {
                commandInfo.ReplyToCommand("DEBUG: Bloque no encontrado");
            }
        });

        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnClientConnected>(OnClientConnected);
        RegisterListener<Listeners.OnClientDisconnectPost>(OnClientDisconnectedPost);

        base.Load(hotReload);
       
    }
    /*
    [GameEventHandler]
    public HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
    {

        Console.WriteLine("hola");

        return HookResult.Continue;
    }
    */
    [GameEventHandler]
    public HookResult EventPlayerTeam(bool force)
    {

        Console.WriteLine("Evento player team triggereado");

        return HookResult.Continue;
    }


    [GameEventHandler]
    public HookResult EventSwitchTeam(bool force)
    {

        Console.WriteLine("Evento Player switch team triggereado");

        return HookResult.Continue;
    }

    [ConsoleCommand("css_revive", "Revives the specified player")]
    [CommandHelper(minArgs: 0, usage: "[name]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnReviveCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        string playerName = commandInfo.GetArg(1);
        if (playerName == "")
        {
            player.Respawn();
        }
        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(playerName, StringComparison.OrdinalIgnoreCase));
        if (target != null)
        {
            target.Respawn();
            commandInfo.ReplyToCommand("ADMIN: " + player.PlayerName + " revivió a " + target.PlayerName);
        }
    }
    [ConsoleCommand("css_swap", "Swap the team of the specified player")]
    [CommandHelper(minArgs: 0, usage: "[name]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnSwapCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        string playerName = commandInfo.GetArg(1);
        if (playerName == "")
        {
            if (player.Team == CsTeam.Terrorist)
            {
                player.ChangeTeam(CsTeam.Terrorist);
                player.Respawn();
            }
            else if (player.Team == CsTeam.CounterTerrorist)
            {
                player.ChangeTeam(CsTeam.Terrorist);
                player.Respawn();
            }
        }

            var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(playerName, StringComparison.OrdinalIgnoreCase));
        if (target != null)
        {
            if (target.Team == CsTeam.Terrorist)
            {
                target.ChangeTeam(CsTeam.CounterTerrorist);
                target.Respawn();
            }
            else if (player.Team == CsTeam.CounterTerrorist)
            {
                target.ChangeTeam(CsTeam.Terrorist);
                target.Respawn();
            }
            target.Respawn();
            commandInfo.ReplyToCommand("ADMIN: " + player.PlayerName + " swapeo a " + target.PlayerName);
        }
    }
}

