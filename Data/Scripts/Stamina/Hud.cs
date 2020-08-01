using Draygo.API;
using Sandbox.ModAPI;
using System;
using System.Text;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Keyspace.Stamina
{
    class Hud
    {
        private HudAPIv2 HudApi;
        private HudAPIv2.HUDMessage hudStaminaReadout;
        private HudAPIv2.BillBoardHUDMessage hudStaminaIcon;
        private HudAPIv2.BillBoardHUDMessage hudStaminaLowOverlay;

        private float stamina;

        internal bool refreshNeeded;

        private Color noColor;

        private const string COLOR_STRING_R = "<color=red>";
        private const string COLOR_STRING_Y = "<color=yellow>";
        private const string COLOR_STRING_W = "<color=white>";
        private string colorString;

        public Hud(HudAPIv2 api)
        {
            HudApi = api;

            stamina = 0;
            refreshNeeded = false;

            noColor = new Color(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));

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
            stamina = num;
            refreshNeeded = true;
        }

        /// <summary>
        /// Refresh the stats display using latest data.
        /// </summary>
        public void Refresh()
        {
            // quality quarantined
            if (!HudApi.Heartbeat)
            {
                return;
            }

            // Check if HUD elements have been initialised. If not, do.
            // TODO: Init once, don't check.
            if (hudStaminaReadout == null)
            {
                hudStaminaReadout = new HudAPIv2.HUDMessage(
                    Message: new StringBuilder(COLOR_STRING_Y.Length + "100".Length),
                    Origin: new Vector2D(-0.925d, 0.95d),
                    Blend: BlendTypeEnum.PostPP
                    );
            }
            if (hudStaminaIcon == null)
            {
                // TODO: Refresh on "exit from any menu", like Digi does in PaintGun (or better!).
                var viewportSize = MyAPIGateway.Session.Camera.ViewportSize;
                float aspectRatio = viewportSize.X / viewportSize.Y;
                hudStaminaIcon = new HudAPIv2.BillBoardHUDMessage(
                    Material: MyStringId.GetOrCompute("StaminaIconMale"),
                    Origin: new Vector2D(-0.95d, 0.935d),
                    BillBoardColor: Color.White,
                    Width: 0.0325f / aspectRatio,
                    Height: 0.0325f,
                    Blend: BlendTypeEnum.PostPP
                    );
            }
            if (hudStaminaLowOverlay == null)
            {
                hudStaminaLowOverlay = new HudAPIv2.BillBoardHUDMessage(
                    Material: MyStringId.GetOrCompute("StaminaLowOverlay"),
                    Origin: new Vector2D(0.0d, 0.0d),
                    BillBoardColor: noColor,
                    Width: 2.0f,
                    Height: 2.0f,
                    Blend: BlendTypeEnum.Standard
                    );
            }

            if (stamina < 0.25f)
            {
                // Sigmoid - so it reaches near-full saturation earlier than stamina gets to zero.
                // See WolframAlpha: 1 - 1/(1+e^(-50x + 6.25)) from -0.25 to 0.25
                // https://www.wolframalpha.com/input/?i=1+-+1%2F%281%2Be%5E%28-50x+%2B+6.25%29%29+from+-0.25+to+0.25
                float alpha = 1.0f - 1.0f / (1.0f + (float)Math.Exp(-50.0f * stamina + 6.25f));
                // Clamp to below 1 so it's never fully opaque.
                alpha = Math.Min(0.95f, Math.Max(0.0f, alpha));
                hudStaminaLowOverlay.BillBoardColor = new Color(new Vector4(1.0f, 1.0f, 1.0f, alpha));
            }
            else
            {
                hudStaminaLowOverlay.BillBoardColor = noColor;
            }

            hudStaminaReadout.Message.Clear();
            
            if (stamina > 0.75f)
                colorString = COLOR_STRING_W;
            else if (stamina < 0.25f)
                colorString = COLOR_STRING_R;
            else
                colorString = COLOR_STRING_Y;

            hudStaminaReadout.Message.AppendFormat($"{colorString}{Convert.ToInt32(stamina * 100.0f)}");

            refreshNeeded = false;
        }
    }
}
