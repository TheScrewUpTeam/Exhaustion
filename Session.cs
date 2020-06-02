using Draygo.API;
using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;

using Stamina.Hud;

namespace Keyspace.Stamina
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Stamina_Session : MySessionComponentBase
    {
        public static Stamina_Session Instance;

        HudAPIv2 HudApi;
        Hud HUD;

        private List<IMyPlayer> PlayerList;

        //public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        //{
        //    base.Init(sessionComponent);
        //}

        public override void LoadData()
        {
            Instance = this;

            // TODO: Player-only
            HudApi = new HudAPIv2();
            HUD = new Hud(HudApi);

            // TODO: Offline or dedicated server
            PlayerList = new List<IMyPlayer>();
        }

        public override void BeforeStart()
        {
            UpdatePlayerList();
        }

        protected override void UnloadData()
        {
            Instance = null;

            HudApi.Unload();

            PlayerList.Clear();
        }

        //public override void HandleInput()
        //{
        //    // gets called 60 times a second before all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        //}

        public override void UpdateAfterSimulation()
        {
            try
            {
                UpdatePlayerList();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
            }
        }

        public override void Draw()
        {
            // TODO: not on DS
            if (HudApi != null && HudApi.Heartbeat)
            {
                HUD.Update(PlayerList.Count);
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
    }
}
