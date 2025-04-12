using Draygo.API;
using Sandbox.ModAPI;
using System.Text;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Keyspace.Stamina
{
    class Hud
    {
        private float iconWidth = 0.03f;
        private float iconHeight = 0.03f;
        private float barWidth = 0.2f;
        private float barHeight = 0.035f;
        private float spacing = 0.01f;
        private Vector2 basePos = new Vector2(-0.7f, -0.63f); // bottom-left corner-ish

        private HudAPIv2.HUDMessage hudStaminaLabel;
        private HudAPIv2.BillBoardHUDMessage hudStaminaFill;

        private float stamina;

        internal bool refreshNeeded;

        private bool lowStaminaWarned = false;

        public Hud()
        {
            stamina = 0;
            refreshNeeded = false;
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
        }

        public void RenderNew()
        {
            var viewportSize = MyAPIGateway.Session.Camera.ViewportSize;
            float aspectRatio = viewportSize.X / viewportSize.Y;

            iconWidth = iconWidth / aspectRatio;

            Vector2 barPos = basePos + new Vector2(iconWidth + barWidth / 2f, 0f);
            Vector2 labelPos = barPos + new Vector2(barWidth + spacing + -0.1f, 0.013f); // slight vertical adjust

            // Background
            new HudAPIv2.BillBoardHUDMessage(
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
            new HudAPIv2.BillBoardHUDMessage(
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
        }
    }
}
