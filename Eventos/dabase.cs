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

public static class dabase
{
    private static readonly string databasepath = Path.Combine("userdata.db");

	public static void Initialize()
	{
        if (!File.Exists("userdata.db"))
        {
            SQLiteConnection.CreateFile("userdata.db");
        }


    }
}
