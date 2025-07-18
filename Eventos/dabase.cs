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
using Microsoft.Data.Sqlite;
using Dapper;


namespace Plugintest;

public class PlayerData
{
    public string SteamID { get; set; }
    public int Experience { get; set; }
    public int Level { get; set; }
    public int Resets { get; set; }

    // UPGRADES!!! (dmg = DAMAGE, hp = HEALTH POINTS, vl = VELOCITY) (H = HUMAN / Z = ZOMBIE)
    public int AvailableH {  get; set; }
    public int AvailableZ { get; set; }
    public int Hdmg { get; set; }
    public int Zdmg { get; set; }

    public int Hhp { get; set; }
    public int Zhp { get; set; }

    public int Hvl { get; set; }
    public int Zvl { get; set; }

    // PREFERENCES / CONFIG
    public string SavedHumanClass { get; set; }
    public string SavedZombieClass { get; set; }
    public string SavedShopPreferencesPrimary { get; set; }
    public string SavedShopPreferencesSecondary { get; set; }
    public string SavedFavoriteColour { get; set; }
}

public static class dabase
{
    private static readonly string databasedirectory = AppContext.BaseDirectory;
    private static readonly string databasepath = Path.Combine(databasedirectory, "userdata.db");

    public static void Initialize()
    {
        if (!File.Exists(databasepath))
        {
            Console.WriteLine("DEBUG: No se encontró una database, creando una");
            using (var connection = new SqliteConnection($"Data Source = {databasepath};"))
            {
                connection.Open();

                string createTable = @"
                    CREATE TABLE IF NOT EXISTS Players (
                    SteamID TEXT PRIMARY KEY,
                    Experience INTEGER NOT NULL,
                    Level INTEGER NOT NULL,
                    Resets INTEGER NOT NULL,
                    UNIQUE(SteamID)
                    );";

                string createTablePlayersConfig = @"
                    CREATE TABLE IF NOT EXISTS PlayersConfig (
                    SteamID TEXT PRIMARY KEY,
                    SavedHumanClass TEXT,
                    SavedZombieClass TEXT,
                    SavedShopPreferencesPrimary TEXT,
                    SavedShopPreferencesSecondary TEXT,
                    SavedFavoriteColour TEXT,
                    FOREIGN KEY (SteamID) REFERENCES Players(SteamID)
                    );";

                string createTablePlayersUpgrades = @"
                    CREATE TABLE IF NOT EXISTS PlayersUpgrades (
                    SteamID TEXT PRIMARY KEY,
                    AvailableH INTEGER NOT NULL,
                    AvailableZ INTEGER NOT NULL,
                    Hdmg INTEGER NOT NULL,
                    Zdmg INTEGER NOT NULL,
                    Hhp INTEGER NOT NULL,
                    Zhp INTEGER NOT NULL,
                    Hvl INTEGER NOT NULL,
                    Zvl INTEGER NOT NULL,
                    FOREIGN KEY (SteamID) REFERENCES Players(SteamID)
                    );";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = createTable;
                    Console.WriteLine("DEBUG: Creando tabla de userdata");
                    command.ExecuteNonQuery();

                    try
                    {
                        command.CommandText = createTablePlayersConfig;
                        Console.WriteLine("DEBUG: Creando tabla de players config");
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error desconocido para players config");
                    }

                    command.CommandText = createTablePlayersUpgrades;
                    Console.WriteLine("DEBUG: Creando tabla de players upgrades");
                    command.ExecuteNonQuery();
                }
                Console.WriteLine("DEBUG: Base de datos y tablas creadas");
            }
        }
        else
        {
            Console.WriteLine("DEBUG: Database encontrada");
        }
    }

    public static PlayerData InitializePlayer(string SteamID)
    {
        using (var connection = new SqliteConnection($"Data Source = {databasepath};"))
        {
            connection.Open();

            string findPlayer = @"
                SELECT * FROM Players WHERE SteamID = @SteamID";

            string findPlayerConfig = @"
                SELECT * FROM PlayersConfig WHERE SteamID = @SteamID";

            string findPlayerUpgrades = @"
                SELECT * FROM PlayersUpgrades WHERE SteamID = @SteamID";

            var player = connection.QueryFirstOrDefault<PlayerData>(findPlayer, new { SteamID = SteamID });
            var playerConfig = connection.QueryFirstOrDefault<PlayerData>(findPlayerConfig, new { SteamID = SteamID });
            var playerUpgrades = connection.QueryFirstOrDefault<PlayerData>(findPlayerUpgrades, new { SteamID = SteamID });

            if (player != null && playerConfig != null && playerUpgrades != null)
            {
                Console.WriteLine("DEBUG: Player encontrado en la base de datos");
                PlayerData connectedPlayer = new PlayerData();
                MergeProperties(player, connectedPlayer);
                MergeProperties(playerConfig, connectedPlayer);
                MergeProperties(playerUpgrades, connectedPlayer);
                return connectedPlayer;
            }
            else
            {
                Console.WriteLine("DEBUG: Jugador no encontrado en la base de datos, creando uno.");

                string addPlayer = @"
                INSERT INTO Players (SteamID, Experience, Level, Resets) VALUES (@SteamID, @Experience, @Level, @Resets)";

                string addPlayerConfig = @"
                INSERT INTO PlayersConfig (SteamID, SavedHumanClass, SavedZombieClass, SavedShopPreferencesPrimary, SavedShopPreferencesSecondary, SavedFavoriteColour) 
                VALUES (@SteamID, @SavedHumanClass, @SavedZombieClass, @SavedShopPreferencesPrimary, @SavedShopPreferencesSecondary, @SavedFavoriteColour)";

                string addPlayerUpgrades = @"
                INSERT INTO PlayersUpgrades (SteamID, AvailableH, AvailableZ, Hdmg, Zdmg, Hhp, Zhp, Hvl, Zvl)
                VALUES (@SteamID, @AvailableH, @AvailableZ, @Hdmg, @Zdmg, @Hhp, @Zhp, @Hvl, @Zvl)";

                var newplayer = new PlayerData
                {
                    SteamID = SteamID,
                    Experience = 0,
                    Level = 1,
                    Resets = 0,
                    // upgrades
                    AvailableH = 0,
                    AvailableZ = 0,
                    Hdmg = 0,
                    Zdmg = 0,
                    Hhp = 0,
                    Zhp = 0,
                    Hvl = 0,
                    Zvl = 0,
                    // Preferences / config
                    SavedHumanClass = "Scalper",
                    SavedZombieClass = "Rezagado",
                    SavedShopPreferencesPrimary = null,
                    SavedShopPreferencesSecondary = null,
                    SavedFavoriteColour = null
                };

                connection.Execute(addPlayer, newplayer);
                connection.Execute(addPlayerConfig, newplayer);
                connection.Execute(addPlayerUpgrades, newplayer);
                Console.WriteLine("DEBUG: Jugador creado con exito");
                return newplayer;
            }
        }
    }

    public static void SavePlayer(PlayerData player)
    {
        var SteamID = player.SteamID;
        using (var connection = new SqliteConnection($"Data Source = {databasepath};"))
        {
            connection.Open();

            string findPlayer = @"
                SELECT * FROM Players WHERE SteamID = @SteamID";

            var storedplayer = connection.QueryFirstOrDefault<PlayerData>(findPlayer, new { SteamID = SteamID });

            string savePlayer = @"
                UPDATE Players
                SET Experience = @Experience,
                    Level = @Level,
                    Resets = @Resets
                WHERE SteamID = @SteamID";

            string savePlayerConfig = @"
                UPDATE PlayersConfig
                SET SavedHumanClass = @SavedHumanClass,
                    SavedZombieClass = @SavedZombieClass,
                    SavedShopPreferencesPrimary = @SavedShopPreferencesPrimary,
                    SavedShopPreferencesSecondary = @SavedShopPreferencesSecondary,
                    SavedFavoriteColour = @SavedFavoriteColour
                WHERE SteamID = @SteamID";

            string savePlayerUpgrades = @"
                UPDATE PlayersUpgrades
                SET AvailableH = @AvailableH,
                    AvailableZ = @AvailableZ,
                    Hdmg = @Hdmg,
                    Zdmg = @Zdmg,
                    Hhp = @Hhp,
                    Zhp = @Zhp,
                    Hvl = @Hvl,
                    Zvl = @Zvl
                WHERE SteamID = @SteamID";

            using (var transaction = connection.BeginTransaction())
            {
                connection.Execute(savePlayer, player, transaction);
                connection.Execute(savePlayerConfig, player, transaction);
                connection.Execute(savePlayerUpgrades, player, transaction);

                transaction.Commit();
            }
            Console.WriteLine("DEBUG: Jugador con steamID " + player.SteamID + " guardo sus datos al salir");
        }
    }

    public static void MergeProperties<T>(T Source, T Target)
    {
        foreach (var prop in typeof(T).GetProperties())
        {
            var value = prop.GetValue(Source);
            if (value != null)
            {
                prop.SetValue(Target, value);
            }
        }
    }
}
