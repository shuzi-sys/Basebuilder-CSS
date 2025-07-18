using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;

namespace Plugintest;

public partial class Plugintest
{

    public void OnClientConnected(int playerSlot)
    {
        Console.WriteLine("hola jugador conectado");
        jugadores = Utilities.GetPlayers();
        Console.WriteLine("Intento de registro de jugador nuevo");
    }

    public void OnClientDisconnectedPost(int playerSlot)
    {
        Console.WriteLine("Adios jugador desconectado listener.");
        jugadores = Utilities.GetPlayers();

    }

    [GameEventHandler]
    public HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        Console.WriteLine("RONDA INICIADA DESDE EVENTORUNDSTART!!!!!!!!!!!");
        Server.ExecuteCommand("exec autoexec.cfg");
        // Removing previous timers if rounds have restarted between build / prep times. It prevents a bug where the build/prep time passes faster (since the previous timer wasnt cleared)
        if (timer != null)
        {
            timer.Kill();
        }

        // Choosing rounds with chances
        Random rand = new Random();
        int totalroundprobabilities = roundModes.Values.Sum();
        int roll = rand.Next(1, totalroundprobabilities + 1);
        int totalprobabilities = 0;
        foreach (var pair in roundModes)
        {
            totalprobabilities += pair.Value;
            if (roll <= totalprobabilities)
            {
                this.selectedMode = pair.Key;
                break;
            }
        }

        Console.WriteLine("Ronda elegida es" + this.selectedMode);
        int normal = 1, realistic = 2, survivor = 3, nemesis = 4;
        switch (this.selectedMode)
        {
            case 1:
                Server.PrintToChatAll("ronda normal");
                this.buildtime = 170;
                this.preptime = 40;
                this.isbuildtime = true;
                buildTimer();
                break;

            case 2:
                Server.PrintToChatAll("ronda realista");
                this.buildtime = 75;
                this.preptime = 15;
                this.isbuildtime = true;
                buildTimer();
                break;
            case 3:
                Server.PrintToChatAll("ronda survivor");
                this.buildtime = 90;
                this.preptime = 40;
                this.isbuildtime = true;
                buildTimer();
                break;
            case 4:
                Server.PrintToChatAll("ronda nemesis");
                this.buildtime = 170;
                this.preptime = 40;
                this.isbuildtime = true;
                buildTimer();
                break;
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        var name = player.PlayerName;
        var playerid = player.SteamID.ToString();
        Console.WriteLine("Se capturo al usuario" + name + "con steamid " + playerid);
        var playerdata = dabase.InitializePlayer(playerid);
        accounts.Add(player, playerdata);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult EventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        var playerdata = accounts[player];
        if (player != null && player.IsValid && player.PawnIsAlive && player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist) 
        {
            clearClaimsByPlayer(player);
        }
        dabase.SavePlayer(playerdata);
        accounts.Remove(player);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult EventPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
            Console.WriteLine("Evento Player Spawn triggereado");
            var player = @event.Userid;
            Console.WriteLine("Spawneo " + player.PlayerName);
            if (player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist)
            {
                Console.WriteLine("Humano");
            }
            if (player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist)
            {
            Server.NextFrame(() =>
            {
                Console.WriteLine("Intentando sobreescribir vida");
                player.SetHP(3000 + 1500);

            });
            }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult EventRoundEnd(EventRoundStart @event, GameEventInfo info)
    {
        Console.WriteLine("DEBUG: Ronda terminada");
        clearClaims();
        clearColors();
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult EventRoundStartPostNav(EventRoundStart @event, GameEventInfo info)
    {
        Console.WriteLine("DEBUG: Ronda comenzada");
        Random random = new Random();
        foreach (var player in Utilities.GetPlayers().Where(p => p != null))
        {
            Console.WriteLine("Accediendo al bucle de jugadores");
            if (player.TeamNum == 3)
            {
                int randomIndex = random.Next(0, colors.Count);
                playercolor.Add(player, colors[randomIndex]);
                player.PrintToCenter("Tu color es " + colors[randomIndex].Name);
                Console.WriteLine("El color de " + player.PlayerName + " es " + colors[randomIndex]);
            }

        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult EventPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        Console.WriteLine("Dañaste a un jugador");

        return HookResult.Continue;
    }

    public void clearClaims()
    {
        claimedblocks.Clear();
        holdinglist.Clear();
        Console.WriteLine("DEBUG: Claims eliminados con exito");
    }

    public void clearClaimsByPlayer(CCSPlayerController player)
    {
        var pendingremove = new List<CBaseProp>();

        foreach (var block in claimedblocks)
        {
            if (block.Value.getowner() == player)
            {
                pendingremove.Add(block.Key);
            }
        }
        foreach(var key in pendingremove)
        {
            claimedblocks.Remove(key);
        }
    }

    public void clearColors()
    {
        playercolor.Clear();
        Console.WriteLine("DEBUG: Colores limpiados con exito");
    }

    public void buildTimer()
    {
        this.timer = AddTimer(1, () =>
        {
            if (isbuildtime)
            {
                if (this.buildtime > 0) this.buildtime--;

                else if (this.buildtime == 0)
                {
                    this.isbuildtime = false;
                    this.ispreptime = true;
                    foreach (var player in jugadores)
                    {
                        if (player == null || player.IsValid) continue;
                        if (player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist)
                        {
                            MenuUI.BuyMenuUI(this, player);
                        }
                        player.Respawn();
                    }
                }
            }

            if (ispreptime)
            {
                if (this.preptime > 0) this.preptime--;

                else if (this.preptime == 0)
                {
                    this.ispreptime = false;
                    timer.Kill();
                    TeleportToLobby(CsTeam.Terrorist);
                }
             }
        }, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);

    }

    public void TeleportToLobby(CsTeam Team)
    {

        Vector destination = Vector.Zero;

        foreach (var entity in Utilities.FindAllEntitiesByDesignerName<CInfoTeleportDestination>("info_teleport_destination"))
        {
            if (entity != null && entity.IsValid && entity.Entity != null && entity.Entity.Name != null && entity.Entity.Name.Contains("teleport_lobby"))
            {
                destination = entity.AbsOrigin!;
                break;
            }
        }

        destination.Z += 20;

        foreach (var player in Utilities.GetPlayers().Where(p => p != null && p.IsValid && p.PlayerPawn.IsValid && p.Connected == PlayerConnectedState.PlayerConnected && p.Team == Team))
        {
            player.PlayerPawn.Value!.Teleport(destination);
        }
    }
}


