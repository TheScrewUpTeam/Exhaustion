using Draygo.API;
using Sandbox.ModAPI;
using System;
using System.Text;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace TSUT.Exhaustion
{
    class Hud
    {
        private float iconWidth = 0.03f;
        private float iconHeight = 0.03f;
        private float barWidth = 0.2f;
        private float barHeight = 0.035f;
        private float spacing = 0.01f;
        private Vector2 basePos = new Vector2(-0.7f, -0.60f); // bottom-left corner-ish

        private HudAPIv2.HUDMessage hudStaminaLabel;
        private HudAPIv2.BillBoardHUDMessage hudStaminaFill;
        private HudAPIv2.BillBoardHUDMessage hudStaminaLowOverlay;
        private HudAPIv2.BillBoardHUDMessage hudIcon;
        private HudAPIv2.BillBoardHUDMessage hudBackground;

        private float stamina;

        internal bool refreshNeeded;

        private bool lowStaminaWarned = false;
        private Color noColor;
        private bool onceRendered = false;

        public Hud()
        {
            var viewportSize = MyAPIGateway.Session.Camera.ViewportSize;
            float aspectRatio = viewportSize.X / viewportSize.Y;

            iconWidth = iconWidth / aspectRatio;

            stamina = 0;
            refreshNeeded = false;
            
            noColor = new Color(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        public void ReactOnSettings(Config config) {
            if (config.ShowSeparateIndicator && onceRendered && hudStaminaLabel == null) {
                RenderNew();
            }
            if (!config.ShowSeparateIndicator && hudStaminaLabel != null) {
                ClearInterface();
            }
        }

        private void ClearInterface()
        {
            hudStaminaLowOverlay.DeleteMessage();
            hudStaminaFill.DeleteMessage();
            hudIcon.DeleteMessage();
            hudBackground.DeleteMessage();
            hudStaminaLabel.DeleteMessage();
            hudStaminaLabel = null;
            hudStaminaFill = null;
            hudStaminaLowOverlay = null;
            hudIcon = null;
            hudBackground = null;
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
            if (hudStaminaFill != null && hudStaminaLabel != null) {
                UpdateNew();
                refreshNeeded = false;
            }
        }

        private void UpdateNew()
        {
            float fillWidth = barWidth * stamina;
            Vector2 barPos = basePos + new Vector2(iconWidth + barWidth / 2f, 0f);
            Vector2 adjustedFillPos = barPos + new Vector2((fillWidth - barWidth) / 2f, 0f);

            hudStaminaFill.Origin = adjustedFillPos;
            hudStaminaFill.Width = barWidth * stamina;
            hudStaminaLabel.Message.Clear();
            hudStaminaLabel.Message.AppendFormat($"{(int)(stamina * 100)}");

            if (stamina < .25f && !lowStaminaWarned) {
                MyAPIGateway.Utilities.ShowNotification("You're critically tired!", 5000, "Red");
                hudStaminaFill.BillBoardColor = Color.Red;
                lowStaminaWarned = true;
            } 
            if (stamina >= .25f) {
                lowStaminaWarned = false;
                hudStaminaFill.BillBoardColor = Color.White;
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
        }

        public void RenderNew()
        {
            onceRendered = true;

            Vector2 barPos = basePos + new Vector2(iconWidth + barWidth / 2f, 0f);
            Vector2 labelPos = barPos + new Vector2(barWidth + spacing + -0.1f, 0.013f); // slight vertical adjust

            // Background
            hudBackground = new HudAPIv2.BillBoardHUDMessage(
                Material: MyStringId.GetOrCompute("Square"),
                Origin: barPos,
                BillBoardColor: Color.Black * 0.5f,
                Width: barWidth,
                Height: barHeight,
                HideHud: false,
                Shadowing: true,
                Blend: BlendTypeEnum.PostPP);

            float fillWidth = barWidth * stamina;
            Vector2 adjustedFillPos = barPos + new Vector2((fillWidth - barWidth) / 2f, 0f);

            // Fill bar
            hudStaminaFill = new HudAPIv2.BillBoardHUDMessage(
                Material: MyStringId.GetOrCompute("Square"),
                Origin: adjustedFillPos,
                BillBoardColor: Color.White,
                Width: fillWidth,
                Height: barHeight,
                HideHud: false,
                Shadowing: true,
                Blend: BlendTypeEnum.PostPP
                );

            // Icon
            hudIcon = new HudAPIv2.BillBoardHUDMessage(
                Material: MyStringId.GetOrCompute("StaminaIconMale"),
                Origin: basePos,
                BillBoardColor: Color.White,
                Width: iconWidth,
                Height: iconHeight,
                HideHud: false,
                Shadowing: true,
                Blend: BlendTypeEnum.PostPP
                );

            // Label
            hudStaminaLabel = new HudAPIv2.HUDMessage
            {
                Message = new StringBuilder(3),
                Origin = labelPos,
                Blend = BlendTypeEnum.PostPP,
                ShadowColor = Color.Black
            };

            // Low stamina overlay
            hudStaminaLowOverlay = new HudAPIv2.BillBoardHUDMessage(
                Material: MyStringId.GetOrCompute("StaminaLowOverlay"),
                Origin: new Vector2D(0.0d, 0.0d),
                BillBoardColor: noColor,
                Width: 2.0f,
                Height: 2.0f,
                Blend: BlendTypeEnum.Standard
            );
        }
    }
}
