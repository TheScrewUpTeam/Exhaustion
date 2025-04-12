﻿using Draygo.API;
using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Library.Utils;
using VRage.Utils;
using static Draygo.API.HudAPIv2;
using VRageRender.Messages;

namespace Keyspace.Stamina
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Stamina_Session : MySessionComponentBase
    {
        public static Stamina_Session Instance;

        // MAGICNUM 47275: last 5 digits of Workshop-published mod ID.
        internal Networking Networking = new Networking(47275);
        internal Config Config;

        private bool isCreativeGame;
        private bool isServer;
        private bool isDedicated;

        private int updateCounter;
        public const int updatePeriod = 6; // ~100 ms  // TODO: configurable!

        internal HudAPIv2 HudApi;
        internal Hud HUD;
        internal MenuRootCategory _menuRoot;
        internal MenuItem _movementToggle, _workToggle, _drivingToggle;
        internal MenuSliderInput _costLow, _costMedium, _costHigh, _gainLow, _gainMedium, _gainHigh;

        private List<IMyPlayer> PlayerList;
        private Dictionary<ulong, PlayerStats> PlayerStatsDict;

        public override void LoadData()
        {
            Instance = this;

            isCreativeGame = (MyAPIGateway.Session.SessionSettings.GameMode == MyGameModeEnum.Creative);
            isServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
            isDedicated = (MyAPIGateway.Utilities.IsDedicated && isServer);

            MyLog.Default.WriteLineAndConsole($"isCreativeGame: {isCreativeGame}; isServer: {isServer}; isDedicated: {isDedicated}");

            if (!isDedicated)
            {
                HudApi = new HudAPIv2(OnHudReady);
                HUD = new Hud();
            }

            if (isServer)
            {
                updateCounter = 0;
                PlayerList = new List<IMyPlayer>();

                Config = StorageFile.Load<Config>("config.xml");
                PlayerStatsDict = StorageFile.Load<PlayerStatsStorage>("stats.xml").ToDict();

                UpdateSettings();
            }
        }

        private void UpdateSettings() {
            MovementCosts.SetFromConfig(Config);
            WorkCosts.SetFromConfig(Config);
        }

        private void OnHudReady()
        {
            BuildSettingsMenu();
            HUD.RenderNew();
        }

        private float NormalizeToSlider(float currentValue) {
            float minSliderValue = 0.0005f;
            float maxSliderValue = 0.0125f;

            // Normalize the value to range between 0 and 1
            float normalizedValue = (currentValue - minSliderValue) / (maxSliderValue - minSliderValue);

            // Ensure that it stays between 0 and 1
            normalizedValue = Math.Max(0.0f, normalizedValue);
            normalizedValue = Math.Min(1.0f, normalizedValue);

            return normalizedValue;
        }

        private float ConvertBackToOriginalValue(float sliderPercent)
        {
            float minSliderValue = 0.0005f;
            float maxSliderValue = 0.0125f;

            // Scale the slider value back to the original range (0.0005 to 0.0125)
            float originalValue = minSliderValue + (sliderPercent * (maxSliderValue - minSliderValue));

            return originalValue;
        }

        private void BuildSettingsMenu()
        {
            _menuRoot = new MenuRootCategory("Exhaustion Mod", MenuRootCategory.MenuFlag.PlayerMenu, "Exhaustion Mod Settings");

            new MenuItem("<color=yellow>---General settings---", _menuRoot);

            _movementToggle = new MenuItem($"Use Stamina for Movement: {(Config.UseStaminaMovement ? "<color=green>On" : "<color=red>Off")}", _menuRoot, () => {
                Config.UseStaminaMovement = !Config.UseStaminaMovement;
                _movementToggle.Text = $"Use Stamina for Movement: {(Config.UseStaminaMovement ? "<color=green>On" : "<color=red>Off")}";
                UpdateSettings();
            });
            _workToggle = new MenuItem($"Use Stamina for Work: {(Config.UseStaminaWork ? "<color=green>On" : "<color=red>Off")}", _menuRoot, () => {
                Config.UseStaminaWork = !Config.UseStaminaWork;
                _workToggle.Text = $"Use Stamina for Work: {(Config.UseStaminaWork ? "<color=green>On" : "<color=red>Off")}";
                UpdateSettings();
            });
            _drivingToggle = new MenuItem($"Use Stamina for Driving: {(Config.UseStaminaDriving ? "<color=green>On" : "<color=red>Off")}", _menuRoot, () => {
                Config.UseStaminaDriving = !Config.UseStaminaDriving;
                _drivingToggle.Text = $"Use Stamina for Driving: {(Config.UseStaminaDriving ? "<color=green>On" : "<color=red>Off")}";
                UpdateSettings();
            });

            new MenuItem("<color=yellow>---Action costs---", _menuRoot);

            _costLow = new MenuSliderInput($"Cost Low: <color=green>{Config.CostLow * 400}/s", _menuRoot, NormalizeToSlider(Math.Abs(Config.CostLow)), "Low Actions Cost/s", (v) => {
                Config.CostLow = ConvertBackToOriginalValue(v) * -1;
                _costLow.Text = $"Cost Low: <color=green>{Config.CostLow * 400}/s";
                _costLow.InitialPercent = NormalizeToSlider(Math.Abs(Config.CostLow));
                UpdateSettings();
            }, (p) => {
                var converted = ConvertBackToOriginalValue(p);
                return ConvertBackToOriginalValue(p) * -400;
            });

            _costMedium = new MenuSliderInput($"Cost Medium: <color=green>{Config.CostMedium * 400}/s", _menuRoot, NormalizeToSlider(Math.Abs(Config.CostMedium)), "Medium Actions Cost/s", (v) => {
                Config.CostMedium = ConvertBackToOriginalValue(v) * -1;
                _costMedium.Text = $"Cost Medium: <color=green>{Config.CostMedium * 400}/s";
                _costMedium.InitialPercent = NormalizeToSlider(Math.Abs(Config.CostMedium));
                UpdateSettings();
            }, (p) => {
                var converted = ConvertBackToOriginalValue(p);
                return ConvertBackToOriginalValue(p) * -400;
            });

            _costHigh = new MenuSliderInput($"Cost High: <color=green>{Config.CostHigh * 400}/s", _menuRoot, NormalizeToSlider(Math.Abs(Config.CostHigh)), "High Actions Cost/s", (v) => {
                Config.CostHigh = ConvertBackToOriginalValue(v) * -1;
                _costHigh.Text = $"Cost High: <color=green>{Config.CostHigh * 400}/s";
                _costHigh.InitialPercent = NormalizeToSlider(Math.Abs(Config.CostHigh));
                UpdateSettings();
            }, (p) => {
                var converted = ConvertBackToOriginalValue(p);
                return ConvertBackToOriginalValue(p) * -400;
            });

            new MenuItem("<color=yellow>---States gains---", _menuRoot);

            _gainLow = new MenuSliderInput($"Gain Low: <color=green>{Config.GainLow * 400}/s", _menuRoot, NormalizeToSlider(Math.Abs(Config.GainLow)), "Low States Gain/s", (v) => {
                Config.GainLow = ConvertBackToOriginalValue(v) * -1;
                _gainLow.Text = $"Gain Low: <color=green>{Config.GainLow * 400}/s";
                _gainLow.InitialPercent = NormalizeToSlider(Math.Abs(Config.GainLow));
                UpdateSettings();
            }, (p) => {
                var converted = ConvertBackToOriginalValue(p);
                return ConvertBackToOriginalValue(p) * -400;
            });
             _gainMedium = new MenuSliderInput($"Gain Medium: <color=green>{Config.GainMedium * 400}/s", _menuRoot, NormalizeToSlider(Math.Abs(Config.GainMedium)), "Medium States Gain/s", (v) => {
                Config.GainLow = ConvertBackToOriginalValue(v) * -1;
                _gainMedium.Text = $"Gain Medium: <color=green>{Config.GainMedium * 400}/s";
                _gainMedium.InitialPercent = NormalizeToSlider(Math.Abs(Config.GainMedium));
                UpdateSettings();
            }, (p) => {
                var converted = ConvertBackToOriginalValue(p);
                return ConvertBackToOriginalValue(p) * -400;
            });
             _gainHigh = new MenuSliderInput($"Gain High: <color=green>{Config.GainHigh * 400}/s", _menuRoot, NormalizeToSlider(Math.Abs(Config.GainHigh)), "High States Gain/s", (v) => {
                Config.GainLow = ConvertBackToOriginalValue(v) * -1;
                _gainHigh.Text = $"Gain High: <color=green>{Config.GainHigh * 400}/s";
                _gainHigh.InitialPercent = NormalizeToSlider(Math.Abs(Config.GainHigh));
                UpdateSettings();
            }, (p) => {
                var converted = ConvertBackToOriginalValue(p);
                return ConvertBackToOriginalValue(p) * -400;
            });

        }

        public override void SaveData()
        {
            if (isServer)
            {
                StorageFile.Save("config.xml", Config);
                StorageFile.Save("stats.xml", new PlayerStatsStorage(PlayerStatsDict));
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
                    // TODO: Update player list less frequently?..
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

        //public override void UpdatingStopped()
        //{
        //    // executed when game is paused
        //}

        private void UpdatePlayerList()
        {
            // TODO: add/remove dynamically?
            PlayerList.Clear();
            // Fetch players that are not bots: they get the same Steam ID as the host/server,
            // and don't have the logic to deal with stats anyway, so skip them.
            MyAPIGateway.Players.GetPlayers(PlayerList, (player) => { return !player.IsBot; });
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

                // In multiplayer mode, the object may not yet be initialised, so only update
                // those with controlled entities.
                if (player?.Controller?.ControlledEntity?.Entity != null)
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
