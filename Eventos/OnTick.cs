using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Extensions;
using System.Numerics;

namespace Plugintest;

public partial class Plugintest
{
    public void OnTick()
    {
        // buildtime/preptime texts
        ShowBuildTime();

        // jugadores != null must be checked in order to proceed to all the methods for players.
        foreach (var player in Utilities.GetPlayers().Where(p => p != null && p.PawnIsAlive))
        {

            bool isPressing = player.Buttons.HasFlag(PlayerButtons.Use);
            bool wasPressing = wasPressingMap.ContainsKey(player) && wasPressingMap[player];

            if (isbuildtime)
            {
                player.PrintToCenterHtml("Build time:" + this.buildtime);
            }

            if (ispreptime)
            {
                player.PrintToCenterHtml("Preparation time:" + this.preptime);
            }

            if (player.Buttons.HasFlag(PlayerButtons.Alt1))
            {
                MenuUI.MainMenuUI(this, player);
            }

            // Checking R usage (Rotate)
            if (player.Buttons.HasFlag(PlayerButtons.Reload))
            {
                if (holdinglist.TryGetValue(player, out var blockg))
                {
                    if (!claimedblocks[blockg].holdingR)
                    {
                        //rotation logic (Also preventing hold / constant use)
                        blockg.Teleport(null, new QAngle(blockg.AbsRotation!.X, blockg.AbsRotation!.Y + 45, blockg.AbsRotation!.Z));
                        claimedblocks[blockg].holdingR = true;
                    }
                }
            }
            // flushing hold
            else
            {
                if (holdinglist.TryGetValue(player, out var blockg))
                {
                    claimedblocks[blockg].holdingR = false;
                }
            }

            // Checking self menu 
            if (player.Buttons.HasFlag(PlayerButtons.Alt1))
            {

            }

            // Checking E usage (Grab)
            if (isPressing && !wasPressing)
            {
                Console.WriteLine("DEBUG: Primer tick del use");

                var block = TraceObject(new CounterStrikeSharp.API.Modules.Utils.Vector(player.PlayerPawn.Value!.AbsOrigin!.X, player.PlayerPawn.Value!.AbsOrigin!.Y, player.PlayerPawn.Value!.AbsOrigin!.Z + player.PlayerPawn.Value.CameraServices!.OldPlayerViewOffsetZ), player.PlayerPawn.Value!.EyeAngles!, false, true);
                if (block == null)
                {
                    Console.WriteLine("DEBUG: No se encontro un bloque");
                    return;
                }
                firstpress(block, player);
            }

            else if (isPressing && wasPressing)
            {
                constantpress(player);
            }

            else if (!isPressing && wasPressing)
            {
                if (holdinglist.TryGetValue(player, out var block))
                {
                    var anchor = claimedblocks[block].anchor;
                    block.AcceptInput("ClearParent");
                    Server.NextWorldUpdate(() =>
                    {
                        anchor.AcceptInput("Kill");
                        Console.WriteLine("DEBUG: Anchor eliminado");
                    });
                    holdinglist.Remove(player);
                    Console.WriteLine("DEBUG: Fin del use");
                    player.ExecuteClientCommand("play sounds/block_drop.wav");
                }
            }
            //flushing hold
            wasPressingMap[player] = isPressing;
        }
    }


    public void firstpress(CBaseProp block, CCSPlayerController player)
    {
        if (claimedblocks.ContainsKey(block))
        {
            if (claimedblocks[block].getowner() == player)
            {
                var hitPoint = TraceShape(new CounterStrikeSharp.API.Modules.Utils.Vector(player.PlayerPawn.Value!.AbsOrigin!.X, player.PlayerPawn.Value!.AbsOrigin!.Y, player.PlayerPawn.Value!.AbsOrigin!.Z + player.PlayerPawn.Value.CameraServices!.OldPlayerViewOffsetZ), player.PlayerPawn.Value!.EyeAngles!, false, true);
                if (hitPoint != null && hitPoint.HasValue)
                {
                    Console.WriteLine("DEBUG: Anchor creado");
                    var anchor = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
                    if (anchor != null && anchor.IsValid)
                    {
                        anchor.Render = Color.Transparent;
                        Server.NextFrame(() => Utilities.SetStateChanged(anchor, "CBaseModelEntity", "m_clrRender"));
                        anchor.DispatchSpawn();
                        anchor.Teleport(Vector3toVector(hitPoint.Value));
                        Console.WriteLine("DEBUG: Anchor tpeado");
                    }
                    int distance = (int)extensiones.distance(anchor.AbsOrigin!, player.PlayerPawn.Value!.AbsOrigin!);
                    claimedblocks[block].anchor = anchor;
                    claimedblocks[block].initDistance = distance;
                    holdinglist.Add(player, block);
                    Server.NextWorldUpdate(() =>
                    {
                        block.AcceptInput("SetParent", anchor, null, "!activator");
                        Console.WriteLine("DEBUG: Anchor parentado con exito");
                    });
                    player.ExecuteClientCommand("play sounds/plugintest/block_grab.wav");
                }
            }
            else
            {
                player.PrintToCenter("Este objeto no te pertenece.");
            }
        }
        else
        {
            var hitPoint = TraceShape(new CounterStrikeSharp.API.Modules.Utils.Vector(player.PlayerPawn.Value!.AbsOrigin!.X, player.PlayerPawn.Value!.AbsOrigin!.Y, player.PlayerPawn.Value!.AbsOrigin!.Z + player.PlayerPawn.Value.CameraServices!.OldPlayerViewOffsetZ), player.PlayerPawn.Value!.EyeAngles!, false, true);
            if (hitPoint != null && hitPoint.HasValue)
            {
                Console.WriteLine("DEBUG: Anchor creado");
                var anchor = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
                if (anchor != null && anchor.IsValid)
                {
                    anchor.Render = Color.Transparent;
                    Server.NextFrame(() => Utilities.SetStateChanged(anchor, "CBaseModelEntity", "m_clrRender"));
                    anchor.DispatchSpawn();
                    anchor.Teleport(Vector3toVector(hitPoint.Value));
                    Console.WriteLine("DEBUG: Anchor tpeado");
                }
                int distance = (int)extensiones.distance(anchor.AbsOrigin!, player.PlayerPawn.Value!.AbsOrigin!);
                var claim = new constructor(player, distance);
                claimedblocks.Add(block, claim);
                holdinglist.Add(player, block);
                claimedblocks[block].anchor = anchor;
                var color = colorcheck(player, block);
                block.Render = color;
                Server.NextWorldUpdate(() =>
                {
                    block.AcceptInput("SetParent", anchor, null, "!activator");
                    Utilities.SetStateChanged(block, "CBaseProp", "m_clrRender");
                    Console.WriteLine("DEBUG: Anchor parentado con exito");
                });
                player.ExecuteClientCommand("play sounds/plugintest/block_grab.wav");
            }

        }
    }

    public void constantpress(CCSPlayerController player)
    {
        if (holdinglist.TryGetValue(player, out var block))
        {
            float initdistance = claimedblocks[block].initDistance;
            var anchor = claimedblocks[block].anchor;
            if (player.Buttons.HasFlag(PlayerButtons.Attack))
            {
                claimedblocks[block].initDistance = initdistance + 3;
            }
            else if (player.Buttons.HasFlag(PlayerButtons.Attack2))
            {
                claimedblocks[block].initDistance = initdistance - 3;
            }
            Console.WriteLine("DEBUG: Recogida del hold con" + initdistance);
            Console.WriteLine("DEBUG: Bloque recogido");
            anchor.Teleport(anchor.NewPos(player, claimedblocks[block].initDistance), null, player.PlayerPawn.Value!.AbsVelocity!);
        }
    }

    public Color colorcheck(CCSPlayerController player, CBaseProp block)
    {
        if (playercolor.ContainsKey(player))
        {
            return playercolor[player];
        }
        else
        {
            Random random = new Random();
            int randomIndex = random.Next(0, colors.Count);
            playercolor.Add(player, colors[randomIndex]);
            return playercolor[player];
        }
    }

    public void ShowBuildTime()
    {

    }
}
