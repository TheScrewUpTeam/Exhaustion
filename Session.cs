using Draygo.API;
using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
//using Sandbox.Game.Screens.Helpers;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Keyspace.Stamina
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Stamina_Session : MySessionComponentBase
    {
        public static Stamina_Session Instance;

        HudAPIv2 HudApi;
        HudAPIv2.HUDMessage message;

        private List<IMyPlayer> PlayerList;

        public override void LoadData()
        {
            Instance = this;

            // TODO: Player-only
            HudApi = new HudAPIv2();

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
                UpdateHud();
            }
        }

        private void UpdateHud()
        {
            if (message == null)
            {
                string TEXT = $"There are {PlayerList.Count} player(s).";
                const double SCALE = 1.0;
                var position = new Vector2D(0.001, 0.001);

                message = new HudAPIv2.HUDMessage(
                    new StringBuilder(TEXT.Length + 24).Append("<color=255,255,0>").Append(TEXT),
                    position,
                    Scale: SCALE,
                    HideHud: true,
                    Blend: BlendTypeEnum.PostPP
                    );
            }
            else
            {
                // ???
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
