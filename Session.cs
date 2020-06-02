using Draygo.API;
using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Input;
using VRage.Library.Utils;
using VRage.Utils;

namespace Keyspace.Stamina
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Stamina_Session : MySessionComponentBase
    {
        public static Stamina_Session Instance;

        public Networking Networking = new Networking(1337);

        private bool isCreativeGame;
        private bool isServer;
        private bool isDedicated;

        int updateCounter;
        int updatePeriod;

        HudAPIv2 HudApi;
        Hud HUD;

        private List<IMyPlayer> PlayerList;

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
                updatePeriod = 300; // 5 seconds // TODO: configurable!
                PlayerList = new List<IMyPlayer>();
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

            HudApi.Unload();

            PlayerList.Clear();
        }

        //public override void HandleInput()
        //{
        //    // gets called 60 times a second before all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        //}

        public override void UpdateAfterSimulation()
        {
            //// example for testing ingame, press L at any point when in a world with this mod loaded
            //// then the server player/console/log will have the message you sent
            //if (MyAPIGateway.Input.IsNewKeyPressed(MyKeys.L))
            //{
            //    Networking.SendToServer(new PacketSimpleExample("L was pressed", 5000));
            //}

            if (isCreativeGame || !isServer)
            {
                return;
            }

            try
            {
                if (updateCounter >= updatePeriod)
                {
                    updateCounter = 0;
                    UpdatePlayerList();
                    SendUpdatesToPlayers();
                }
                else
                {
                    updateCounter++;
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

            if (HudApi != null && HudApi.Heartbeat)
            {
                // FIXME: _specific_ player
                HUD.Update(updateCounter);
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
            PlayerList.Clear();
            MyAPIGateway.Players.GetPlayers(PlayerList);
        }

        private void SendUpdatesToPlayers()
        {
            foreach (IMyPlayer player in PlayerList)
            {
                Networking.SendToPlayer(new PacketSimpleExample("Server says hi.", 5000), player.SteamUserId);
            }
        }
    }
}
