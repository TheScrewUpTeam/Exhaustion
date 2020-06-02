using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace Keyspace.Stamina
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Stamina_Session : MySessionComponentBase
    {
        public static Stamina_Session Instance;
        private List<IMyPlayer> PlayerList = new List<IMyPlayer>();

        public override void LoadData()
        {
            Instance = this;
        }

        public override void BeforeStart()
        {
            UpdatePlayerList();
        }

        protected override void UnloadData()
        {
            Instance = null;
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
                //MyAPIGateway.Utilities.ShowNotification($"There are {playerList.Count} players.", 1000, MyFontEnum.Green);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
            }
        }

        //public override void Draw()
        //{
        //    // gets called 60 times a second after all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        //    // NOTE: this is the only place where the camera matrix (MyAPIGateway.Session.Camera.WorldMatrix) is accurate, everywhere else it's 1 frame behind.
        //}

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
