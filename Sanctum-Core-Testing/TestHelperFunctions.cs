using Sanctum_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Testing
{
    internal class TestHelperFunctions
    {
        public static void AddPlayers(List<string> uuids, List<Playtable> playtables)
        {
            for (int i = 0; i < uuids.Count; ++i)
            {
                string uuid = uuids[i];
                Playtable playtable = playtables[i];
                playtable.AddOrRemovePlayer(uuid, uuid, true);
            }
        }


        public static List<Player> GetAllPlayers(List<Playtable> playtables)
        {
            List<Player> allPlayers = new();
            foreach (Playtable itertable in playtables)
            {
                List<Player> players = (List<Player>)typeof(Playtable).GetField("_players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(itertable) ?? throw new Exception("Could not find players");
                allPlayers.AddRange(players);
            }
            return allPlayers;
        }
        public static List<Player> GetRelevantPlayersFromTables(List<string> uuids, List<Playtable> playtables)
        {
            List<Player> relevantPlayers = new();
            for (int i = 0; i < uuids.Count; ++i)
            {
                string uuid = uuids[i];
                Playtable playtable = playtables[i];
                relevantPlayers.Add(playtable.GetPlayer(uuid));
            }
            return relevantPlayers;
        }

        public static List<Player> GetAllPlayerOfUUID(string uuid, List<Playtable> playtables)
        {
            List<Player> relevantPlayers = new();
            foreach (Playtable playtable in playtables)
            {
                Player? player = playtable.GetPlayer(uuid) ?? throw new Exception("Can't find player");
                relevantPlayers.Add(player);
            }
            return relevantPlayers;
        }
    }
}
