using Draygo.API;
using System;
using System.Text;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Keyspace.Stamina
{
    class Hud
    {
        private HudAPIv2 HudApi;
        private HudAPIv2.HUDMessage hudStaminaMessage;
        private HudAPIv2.BillBoardHUDMessage hudStaminaIcon;

        private int stamina;

        internal bool refreshNeeded;

        private const string COLOR_STRING_R = "<color=red>";
        private const string COLOR_STRING_Y = "<color=yellow>";
        private const string COLOR_STRING_W = "<color=white>";
        private string colorString;

        public Hud(HudAPIv2 api)
        {
            HudApi = api;

            stamina = 0;
            refreshNeeded = false;
            colorString = "";
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
            stamina = Convert.ToInt32(num * 100.0f);
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

            if (hudStaminaMessage == null)
            {
                hudStaminaMessage = new HudAPIv2.HUDMessage(
                    Message: new StringBuilder(COLOR_STRING_Y.Length + "100".Length),
                    Origin: new Vector2D(-0.925, 0.95),
                    Blend: BlendTypeEnum.PostPP
                    );
            }
            if (hudStaminaIcon == null)
            {
                hudStaminaIcon = new HudAPIv2.BillBoardHUDMessage(
                    Material: MyStringId.GetOrCompute("StaminaIconMale"),
                    Origin: new Vector2D(-0.95, 0.935),
                    BillBoardColor: Color.White,
                    Width: 0.025f,
                    Height: 0.0325f,
                    Blend: BlendTypeEnum.PostPP
                    );
            }

            hudStaminaMessage.Message.Clear();
            
            if (stamina > 75)
                colorString = COLOR_STRING_W;
            else if (stamina < 25)
                colorString = COLOR_STRING_R;
            else
                colorString = COLOR_STRING_Y;

            hudStaminaMessage.Message.AppendFormat($"{colorString}{stamina}");

            refreshNeeded = false;
        }
    }
}
