using System;
using System.Drawing;
using System.Collections.Generic;
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Plugintest;

public static class extensiones
{

    
    public static CBaseProp? GetClientAimTarget(this CCSPlayerController Player)
    {
        var GameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;

        if (GameRules == null)
            return null;

        VirtualFunctionWithReturn<IntPtr, IntPtr, IntPtr> findPickerEntity = new(GameRules.Handle, 27);

        var target = new CBaseProp(findPickerEntity.Invoke(GameRules.Handle, Player.Handle));

        if (target != null && target.IsValid && target.Entity != null) return target;
        //&& (target.DesignerName.Contains("prop_dynamic") || target.DesignerName.Contains("prop_physics"))
        else return null;
    }

    public static float distance(Vector Partida, Vector Objetivo)
    {
        float dx = Partida.X - Objetivo.X;
        float dy = Partida.Y - Objetivo.Y;
        float dz = Partida.Z - Objetivo.Z;
        float distance = MathF.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        return distance;
    }

    public static float PropToPlayerDistance(this CBaseProp target, CCSPlayerController Player)
    {
        Vector PlayerPos = Player.PlayerPawn.Value.AbsOrigin;
        Vector PropPos = target.AbsOrigin;
        float dx = PlayerPos.X - PropPos.X;
        float dy = PlayerPos.Y - PropPos.Y;
        float dz = PlayerPos.Z - PropPos.Z;
        float distance = MathF.Sqrt ((dx * dx) + (dy * dy) + (dz * dz));
        return distance;
    }

    public static Vector NewPos(this CBaseProp target, CCSPlayerController Player, float distance)
    {
        double EyeAnglePitch = -Player.PlayerPawn.Value.EyeAngles.X;
        double EyeAngleYaw = Player.PlayerPawn.Value.EyeAngles.Y;
        double RadianEyeAnglePitch = (Math.PI / 180) * EyeAnglePitch;
        double RadianEyeAngleYaw = (Math.PI / 180) * EyeAngleYaw;
        Vector playerpos = Player.PlayerPawn.Value.AbsOrigin;
        // coordenadas nuevas para el block
        double x = playerpos.X + distance * Math.Cos(RadianEyeAnglePitch) * Math.Cos(RadianEyeAngleYaw);
        double y = playerpos.Y + distance * Math.Cos(RadianEyeAnglePitch) * Math.Sin(RadianEyeAngleYaw);
        double z = playerpos.Z + Player.PlayerPawn.Value!.CameraServices!.OldPlayerViewOffsetZ + distance * Math.Sin(RadianEyeAnglePitch);
        return new Vector((float)x, (float)y, (float)z);
    }
    public static Vector NewPosWOffset(this CBaseProp target, CCSPlayerController Player, float distance, Vector Offset)
    {
        double EyeAnglePitch = -Player.PlayerPawn.Value.EyeAngles.X;
        double EyeAngleYaw = Player.PlayerPawn.Value.EyeAngles.Y;
        double RadianEyeAnglePitch = (Math.PI / 180) * EyeAnglePitch;
        double RadianEyeAngleYaw = (Math.PI / 180) * EyeAngleYaw;
        Vector playerpos = Player.PlayerPawn.Value.AbsOrigin;
        // coordenadas nuevas para el block
        double x = playerpos.X + distance * Math.Cos(RadianEyeAnglePitch) * Math.Cos(RadianEyeAngleYaw);
        double y = playerpos.Y + distance * Math.Cos(RadianEyeAnglePitch) * Math.Sin(RadianEyeAngleYaw);
        double z = playerpos.Z + Player.PlayerPawn.Value!.CameraServices!.OldPlayerViewOffsetZ + distance * Math.Sin(RadianEyeAnglePitch);
        Vector result = new Vector((float)x, (float)y, (float)z);
        return result + Offset;
    }

    public static void SetHP(this CCSPlayerController player, int health = 100)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive) return;
        player.Health = health;
        player.PlayerPawn.Value.Health = health;
        if (health > 100)
        {
            player.MaxHealth = health;
            player.PlayerPawn.Value.MaxHealth = health;
        }
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iMaxHealth");
    }

    public static void SetArmor(this CCSPlayerController player, int armor)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive) return;
        if (armor > 0)
        {
            player.PlayerPawn.Value.ArmorValue = armor;
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_ArmorValue");
        }
    }

}

public class constructor
{
    private CCSPlayerController owner;
    public float initDistance;
    public Vector offset;
    public bool holdingR;
    public CDynamicProp anchor;
    public float offsetdistance;

    public constructor(CCSPlayerController passedowner, float passeddistance)
    {    
        this.owner = passedowner;
        this.initDistance = passeddistance;
        this.holdingR = false;
       }


    public CCSPlayerController getowner()
    {
        return owner;
    }
}

