using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Commands;

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
    public HookResult EventPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {

        Console.WriteLine("Evento Player Spawn triggereado");

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
    public void clearColors()
    {
        playercolor.Clear();
        Console.WriteLine("DEBUG: Colores limpiados con exito");
    }
}
