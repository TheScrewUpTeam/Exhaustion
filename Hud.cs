using Draygo.API;
using System;
using System.Text;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Keyspace.Stamina
{
    class Hud
    {
        private HudAPIv2 HudApi;
        private HudAPIv2.HUDMessage hudMessage;

        private int stamina;

        internal bool refreshNeeded;

        public Hud(HudAPIv2 api)
        {
            HudApi = api;

            stamina = 0;
            refreshNeeded = false;
        }

        /// <summary>
        /// Clean up references when exiting game.
        /// </summary>
        public void Unload()
        {
            // we didn't initialise this, so don't unload it either
            HudApi = null;
        }

        /// <summary>
        /// Update tracked data; refreshing the display is done in Refresh().
        /// </summary>
        public void Update(float num)
        {
            stamina = Convert.ToInt32(num);
            refreshNeeded = true;
        }

        /// <summary>
        /// Refresh the stats display using latest data.
        /// </summary>
        public void Refresh()
        {
            if (!HudApi.Heartbeat)
            {
                return;
            }

            if (hudMessage == null)
            {
                hudMessage = new HudAPIv2.HUDMessage(
                    Message: new StringBuilder(5),
                    Origin: new Vector2D(-0.95, 0.95),
                    Scale: 1.0,
                    HideHud: true,
                    Blend: BlendTypeEnum.PostPP
                    );
            }

            hudMessage.Message.Clear();
            hudMessage.Message.Append(stamina);

            refreshNeeded = false;
        }
    }
}
