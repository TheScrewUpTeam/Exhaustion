using Draygo.API;
using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Library.Utils;
using VRage.Utils;

namespace Keyspace.Stamina
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Stamina_Session : MySessionComponentBase
    {
        public static Stamina_Session Instance;

        public Networking Networking = new Networking(31337);

        private bool isCreativeGame;
        private bool isServer;
        private bool isDedicated;

        private int updateCounter;
        public const int updatePeriod = 60; // ~1 second // TODO: configurable!

        internal HudAPIv2 HudApi;
        internal Hud HUD;

        private List<IMyPlayer> PlayerList;
        Dictionary<ulong, PlayerStats> PlayerStatsDict;

        public override void LoadData()
        {
            Instance = this;

            isCreativeGame = (MyAPIGateway.Session.SessionSettings.GameMode == MyGameModeEnum.Creative);
            isServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
            isDedicated = (MyAPIGateway.Utilities.IsDedicated && isServer);

            if (!isDedicated)
            { 
                HudApi = new HudAPIv2();
                HUD = new Hud(HudApi);
            }

            if (isServer)
            {
                updateCounter = 0;
                PlayerList = new List<IMyPlayer>();
                PlayerStatsDict = new Dictionary<ulong, PlayerStats>();
            }
        }

        public override void BeforeStart()
        {
            Networking.Register();

            if (isServer)
            {
                UpdatePlayerList();
            }
        }

        protected override void UnloadData()
        {
            Instance = null;

            Networking?.Unregister();
            Networking = null;

            HUD?.Unload();
            HudApi?.Unload();

            PlayerList?.Clear();
            PlayerStatsDict?.Clear();
        }

        //public override void HandleInput()
        //{
        //    // gets called 60 times a second before all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        //}

        public override void UpdateAfterSimulation()
        {
            if (isCreativeGame || !isServer)
            {
                return;
            }
            
            try
            {
                if (updateCounter < updatePeriod)
                {
                    updateCounter++;
                }
                else
                {
                    updateCounter = 0;
                    UpdatePlayerList();
                    UpdateAllPlayerStats();
                    SendUpdatesToAllPlayers();
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Consider filing a bug report. ]", 10000, MyFontEnum.Red);
            }
        }

        public override void Draw()
        {
            if (isDedicated)
            {
                return;
            }

            // Updates are in packet handlers, just display business here.
            if (HudApi != null && HUD.refreshNeeded)
            {
                HUD.Refresh();
            }
        }

        //public override void SaveData()
        //{
        //    // executed AFTER world was saved
        //}

        //public override void UpdatingStopped()
        //{
        //    // executed when game is paused
        //}

        private void UpdatePlayerList()
        {
            // TODO: add/remove dynamically?
            PlayerList.Clear();
            MyAPIGateway.Players.GetPlayers(PlayerList);
        }

        private void UpdateAllPlayerStats()
        {
            foreach (IMyPlayer player in PlayerList)
            {
                ulong steamId = player.SteamUserId;

                if (!PlayerStatsDict.ContainsKey(steamId))
                {
                    PlayerStatsDict.Add(steamId, new PlayerStats());
                }
                else
                {
                    PlayerStatsDict[steamId].Recalculate(player);
                }
            }
        }

        private void SendUpdatesToAllPlayers()
        {
            foreach (IMyPlayer player in PlayerList)
            {
                ulong steamId = player.SteamUserId;
                Networking.SendToPlayer(
                    new StatsPacket(PlayerStatsDict[steamId].Stamina),
                    steamId
                    );
            }
        }
    }
}
