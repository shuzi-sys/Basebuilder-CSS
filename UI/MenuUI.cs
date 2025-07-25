using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CS2MenuManager.API.Menu;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Drawing;
using Microsoft.Data.Sqlite;
using Dapper;
using Plugintest;


namespace Plugintest;

// All unlockable / purchaseable items inherit from this class.
public class Metadata
{
    public string name {  get; set; }
    public int level { get; set; }
    public int reset { get; set; }

}
// Used for weapon list
public class Weapons : Metadata
{
    public string weaponEntName { get; set; }
}
// Used for human and zombie classes
public class Classes : Metadata
{
    public int classHP { get; set; }
    public string classTextureFile { get; set; }
}



public static class MenuUI
{
    // ADD / REMOVE / MODIFY YOUR STUFF HERE!

    // Weapons
    public static List<Weapons> primaryweaponlist = new()
    {
    new Weapons { name = "Schmidt MP9 (LVL: 1) (R:0)", weaponEntName = "weapon_mp9", level = 1, reset = 0 },
    new Weapons { name = "XM 1014 m4 (LVL: 3) (R:0)", weaponEntName = "weapon_xm1014", level = 3, reset = 0 },
    new Weapons { name = "UMP (LVL: 5) (R:0)", weaponEntName = "weapon_ump", level = 5, reset = 0 },
    new Weapons { name = "Mac 10 (LVL:6) (R:0)", weaponEntName = "weapon_mac10", level = 6, reset = 0 },
    new Weapons { name = "Nova Shotgun (LVL:8) (R:0)", weaponEntName = "weapon_nova", level = 8, reset = 0 },
    new Weapons { name = "Schmidt Scout (LVL:9) (R:0)", weaponEntName = "weapon_ssg08", level = 9, reset = 0 },
    new Weapons { name = "Navy MP5-SD (LVL: 10) (R:0)", weaponEntName = "weapon_mp5sd", level = 10, reset = 0 },
    new Weapons { name = "AWP Magnum Sniper (LVL: 14) (R:0)", weaponEntName = "weapon_awp", level = 14, reset = 0 },
    new Weapons { name = "FN P90 (LVL: 18) (R:0)", weaponEntName = "weapon_p90", level = 18, reset = 0 },
    new Weapons { name = "Heckler koch MP7 (LVL: 21) (R:0)", weaponEntName = "weapon_mp7", level = 21, reset = 0 },
    new Weapons { name = "Famas G2 (LVL: 24) (R:0)", weaponEntName = "weapon_famas", level = 24, reset = 0 },
    new Weapons { name = "IMI Galil (LVL: 28) (R:0)", weaponEntName = "weapon_galil", level = 28, reset = 0 },
    new Weapons { name = "Steyr AUG A1 (LVL: 33) (R:0)", weaponEntName = "weapon_aug", level = 33, reset = 0 },
    new Weapons { name = "Colt M4A1-s Carbine (LVL: 9) (R:1)", weaponEntName = "weapon_m4a1_silencer", level = 9, reset = 1 },
    };

    public static List<Weapons> secondaryweaponlist = new()
    {
        new Weapons { name = "Glock 18 (LVL: 1) (R:0)", weaponEntName = "weapon_glock", level = 1, reset = 0 },
        new Weapons { name = "USP.45 Tactical (LVL: 7) (R:0)", weaponEntName = "weapon_usp_silencer", level = 7, reset = 0},
        new Weapons { name = "P228 Silver (LVL: 15) (R:0)", weaponEntName = "weapon_p250", level = 15, reset = 0},
        new Weapons { name = "Five Seven (LVL: 25) (R:0)", weaponEntName = "weapon_fiveseven", level = 25, reset = 0},
        new Weapons { name = "Dual Berettas (LVL: 32) (R:0)", weaponEntName = "weapon_elite", level = 32, reset = 0},
        new Weapons { name = "R8 Revolver (LVL: 12) (R:1)", weaponEntName = "weapon_revolver", level = 12, reset = 1},
        new Weapons { name = "Tec - 9 (LVL: 20) (R:2)", weaponEntName = "weapon_tec9", level = 20, reset = 2 },
        new Weapons { name = "Desert Eagle.50 (LVL: 8) (R:3)", weaponEntName = "weapon_deagle", level = 8, reset = 3}
    };

    public static List<Weapons> utilitiesweaponlist = new()
    {
        new Weapons { name = "Kit Basico", weaponEntName = "weapon_hegrenade", level = 1, reset = 0 }
    };
    // Human classes
    public static List<Classes> humanslist = new()
    {
    new Classes { name = "Scalper (LVL: 1) (R:0) (HP: 100)", classHP = 100, level = 1, reset = 0, classTextureFile = ""}
    };

    // Zombie classes
    public static List<Classes> zombieslist = new()
    {
    new Classes { name = "Rezagado (LVL: 1) (R:0) (HP: 3000)", classHP = 3000, level = 1, reset = 0, classTextureFile = ""},
    new Classes { name = "agustin ariel calvo (LVL: 1) (R:0) (HP: 3000)", classHP = 666, level = 1, reset = 0, classTextureFile = ""}
    };

    //=======================================================================================================================================



    public static void MainMenuUI(BasePlugin plugin, CCSPlayerController player)
    {
        ScreenMenu menu = new("Menu", plugin);
        menu.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress;
        menu.AddItem("Menu de armas", (p, o) => { BuyMenuUI(plugin, p); });
        menu.AddItem("Items extra", (p, o) => { p.PrintToChat("Esta opcion aun no esta disponible"); });
        menu.AddItem("Clases humanas", (p, o) => { HumanClassMenuUI(plugin, p); });
        menu.AddItem("Clases zombie", (p, o) => { ZombieClassMenuUI(plugin, p); });
        menu.AddItem("Opciones de cuenta", (p, o) => { AccountOptionsUI(plugin, p); });
        menu.Display(player, 10);
    }

    public static void BuyMenuUI(BasePlugin plugin, CCSPlayerController player)
    {

        if (player.Team == CsTeam.CounterTerrorist && player.PawnIsAlive)
        {
            // First primary or secondary?
            ScreenMenu buyMenuUI = new("Menu de armas", plugin);
            buyMenuUI.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress;
            buyMenuUI.AddItem("Armas primarias", (player, option) => { PrimaryWeaponsUI(plugin, player); });
            buyMenuUI.AddItem("Armas secundarias", (player, option) => { SecondaryWeaponsUI(plugin, player); });
            buyMenuUI.AddItem("Utilidades", (player, option) => { UtilitiesWeaponsUI(plugin, player); });
            buyMenuUI.Display(player, 10);
        }
        else
        {
            player.PrintToChat("No cumples las condiciones para esta accion");
        }
    }

    // Weapons Buy Menus
    public static void PrimaryWeaponsUI(BasePlugin plugin, CCSPlayerController player)
    {
        ScreenMenu weaponMenu = new("Armas primarias", plugin);
        weaponMenu.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress;
        foreach (var weapon in primaryweaponlist)
        {
            weaponMenu.AddItem(weapon.name, (player, option) =>
            {
                if (Plugintest.accounts[player].Level == weapon.level && Plugintest.accounts[player].Resets == weapon.reset)
                {
                    player.PrintToChat("Elegiste el" + weapon.name);
                    player.GiveNamedItem(weapon.weaponEntName);
                }
                else
                {
                    player.PrintToChat("No cumples con los requisitos para este arma!");
                }
            });
        }
        weaponMenu.Display(player, 20);
    }

    public static void SecondaryWeaponsUI(BasePlugin plugin, CCSPlayerController player)
    {
        ScreenMenu weaponMenu = new("Armas Secundarias", plugin);
        weaponMenu.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress;
        foreach (var weapon in secondaryweaponlist)
        {
            weaponMenu.AddItem(weapon.name, (player, option) =>
            {
                if (Plugintest.accounts[player].Level == weapon.level && Plugintest.accounts[player].Resets == weapon.reset)
                {
                    player.PrintToChat("Elegiste el" + weapon.name);
                    player.GiveNamedItem(weapon.weaponEntName);
                }
                else
                {
                    player.PrintToChat("No cumples con los requisitos para este arma!");
                }
            });
        }
        weaponMenu.Display(player, 20);
    }

    public static void UtilitiesWeaponsUI(BasePlugin plugin, CCSPlayerController player)
    {
        ScreenMenu weaponMenu = new("Utilidades", plugin);
        weaponMenu.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress;
        foreach (var weapon in utilitiesweaponlist)
        {
            weaponMenu.AddItem(weapon.name, (player, option) =>
            {
                if (Plugintest.accounts[player].Level == weapon.level && Plugintest.accounts[player].Resets == weapon.reset)
                {
                    player.PrintToChat("Elegiste el" + weapon.name);
                    player.GiveNamedItem(weapon.weaponEntName);
                }
                else
                {
                    player.PrintToChat("No cumples con los requisitos para este arma!");
                }
            });
        }
        weaponMenu.Display(player, 20);
    }

    // HUMAN & ZOMBIE Class menus ( Sadly i couldnt figure out a way to not copy and paste this method for humans and zombies, but whatever.)
    public static void HumanClassMenuUI(BasePlugin plugin, CCSPlayerController player)
    {
        ScreenMenu humanMenu = new ScreenMenu("Elige una clase", plugin);
        humanMenu.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress;
        foreach (var humanclass in humanslist)
        {
            humanMenu.AddItem(humanclass.name, (player, option) =>
            {
                // Check for requirements
                if (Plugintest.accounts[player].Level == humanclass.level && Plugintest.accounts[player].Resets == humanclass.reset)
                {
                    player.PrintToChat("Elegiste el " + humanclass.name);
                    Plugintest.accounts[player].HumanClass = humanclass;
                    // The class name must be saved since we use it in the data base!
                    Plugintest.accounts[player].SavedHumanClass = humanclass.name;
                    //
                    if (player.PawnIsAlive && player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist)
                    {
                        player.Respawn();
                    }
                }
                else
                {
                    player.PrintToChat("No cumples con los requisitos para esta clase!");
                }
            });
            
        }
        humanMenu.Display(player, 20);
    }

    public static void ZombieClassMenuUI(BasePlugin plugin, CCSPlayerController player)
    {
        ScreenMenu zombieMenu = new ScreenMenu("Elige una clase", plugin);
        zombieMenu.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress;
        foreach (var zombieclass in zombieslist)
        {
            zombieMenu.AddItem(zombieclass.name, (player, option) =>
            {
                // Check for requirements
                if (Plugintest.accounts[player].Level == zombieclass.level && Plugintest.accounts[player].Resets == zombieclass.reset)
                {
                    player.PrintToChat("Elegiste el " + zombieclass.name);
                    Plugintest.accounts[player].ZombieClass = zombieclass;
                    // The class name must be saved since we use it in the data base!
                    Plugintest.accounts[player].SavedZombieClass = zombieclass.name;
                    //
                    if (player.PawnIsAlive && player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist)
                    {
                        player.Respawn();
                    }
                }
                else
                {
                    player.PrintToChat("No cumples con los requisitos para esta clase!");
                }
            });
            
        }
        zombieMenu.Display(player, 20);
    }




    // Others
    public static void ItemsExtraUI(CCSPlayerController player, CS2MenuManager.API.Class.ItemOption option)
    {

    }

    public static void AccountOptionsUI(BasePlugin plugin, CCSPlayerController player)
    {
        ScreenMenu optionsMenu = new ScreenMenu("Elige una opción", plugin);
        optionsMenu.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress;
        optionsMenu.AddItem("Preferencias de color", (player, option) => { ColoursUI(plugin, player); });
        optionsMenu.AddItem("Estadisticas de mi cuenta", (player, option) => { StatsMenuUI(plugin, player); });
        optionsMenu.Display(player, 20);
    }

    public static void ColoursUI(BasePlugin plugin, CCSPlayerController player)
    {
        ScreenMenu colorsMenu = new ScreenMenu("Elige un color", plugin);
        colorsMenu.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress; 
        foreach (var color in Plugintest.colors)
        {
            colorsMenu.AddItem(color.Name, (player, option) =>
            {
                SetColor(player, color);
                player.PrintToChat("Elegiste el color " +  color.Name);
            });
        }
        colorsMenu.Display(player, 20);
    }

    public static void SetColor(CCSPlayerController player, Color color)
    {
        if (player == null || !player.IsValid) return;
        Plugintest.playercolor[player] = color;
    }

    public static void StatsMenuUI(BasePlugin plugin, CCSPlayerController player)
    {
        ScreenMenu statsMenu = new ScreenMenu("Estadisticas:", plugin);
        statsMenu.ScreenMenu_MenuType = CS2MenuManager.API.Enum.MenuType.KeyPress;
        var playerdata = Plugintest.accounts[player];
        foreach (var property in playerdata.GetType().GetProperties())
        {
            statsMenu.AddItem(property.Name + " " + property.GetValue(playerdata), (player, option) => { player.PrintToChat("foo"); });
        }
        statsMenu.Display(player, 20);
    }

}


