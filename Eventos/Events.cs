using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace Plugintest;

public partial class Plugintest
{
    private HookResult CBaseEntity_TakeDamageOldFunc(DynamicHook hook)
    {
        CEntityInstance ent = null;
        ent = hook.GetParam<CEntityInstance>(0);
        CCSPlayerPawn? playerpawn = ent.As<CCSPlayerPawn>();

        var a = hook.GetParam<CTakeDamageInfo>(1);
        CCSPlayerPawn? attackerpawn = a.Attacker.Value?.As<CCSPlayerPawn>();

        CCSPlayerController? player = playerpawn.OriginalController.Value;
        CCSPlayerController? attacker = attackerpawn.OriginalController.Value;

        if (accounts.ContainsKey(attacker) && playerpawn.IsValid)
        {
            if (attacker.Team == CsTeam.CounterTerrorist)
            {
                a.Damage += ((accounts[attacker].Resets + 1) * (accounts[attacker].Hdmg * 25));
            }
            else
            {
                a.Damage += ((accounts[attacker].Resets + 1) * (accounts[attacker].Zdmg * 25));
            }
            Server.PrintToConsole($"Jugador: {attacker.PlayerName} ataco a: {player.PlayerName} por: {a.Damage}");
            return HookResult.Continue;
        }
        else
        {
            Server.PrintToConsole($"No se pudo encontrar al atacante {attacker.PlayerName} en playeraccounts");
            return HookResult.Continue;
        }
    }
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
        if (!player.IsBot)
        {
            var playerdata = dabase.InitializePlayer(playerid);

            foreach (var humanclass in MenuUI.humanslist)
            {
                if (humanclass.name.Contains(playerdata.SavedHumanClass, StringComparison.OrdinalIgnoreCase))
                {
                    playerdata.HumanClass = humanclass;
                    Console.WriteLine("Se intento colocar la clase humano guardada");
                }
            }

            foreach (var zombieclass in MenuUI.zombieslist)
            {
                if (zombieclass.name.Contains(playerdata.SavedZombieClass, StringComparison.OrdinalIgnoreCase))
                {
                    playerdata.ZombieClass = zombieclass;
                    Console.WriteLine("Se intento colocar la clase zombie guardada");
                }
            }

            accounts.Add(player, playerdata);
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult EventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsBot)
        {
            var playerdata = accounts[player];
            if (player != null && player.IsValid && player.PawnIsAlive && player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist)
            {
                clearClaimsByPlayer(player);
            }
            dabase.SavePlayer(playerdata);
            accounts.Remove(player);
            
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult EventPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        Console.WriteLine("Evento Player Spawn triggereado");
        var player = @event.Userid;
        if (!player.IsBot)
        {
            var playerdata = accounts[player];
            Console.WriteLine("Spawneo " + player.PlayerName);

            if (player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist)
            {
                Server.NextFrame(() =>
                {
                    player.SetHP(playerdata.HumanClass.classHP);
                    Console.WriteLine("Humano con clase: " + playerdata.HumanClass.name);
                });
            }

            if (player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist)
            {
                Server.NextFrame(() =>
                {
                    player.SetHP(playerdata.ZombieClass.classHP);
                    Console.WriteLine("Zombie con clase: " + playerdata.ZombieClass.name);
                });
            }
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult EventRoundEnd(EventRoundStart @event, GameEventInfo info)
    {
        var eventname = @event.EventName;
        Console.WriteLine("DEBUG: Event round end con nombre " +  eventname);
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
        @event.DmgHealth = 1000;
        return HookResult.Changed;
    }

    [GameEventHandler]
    public HookResult EventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        switch (selectedMode) {
            case 1: /* normal round */

                // handle usual zombie respawn
                if (player != null && player.Team == CsTeam.Terrorist)
                {
                    var timer = AddTimer(2, () =>
                    {
                        player.Respawn();
                    });
                }
                break;

            case 2: /* realistic round */

                break;

            case 3: /* survivor round */

                break;

            case 4: /* nemesis round */

                break;
        }

        return HookResult.Continue;
    }

    // OTHERS
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
                    }
                    TeleportToLobby(CsTeam.CounterTerrorist);
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


