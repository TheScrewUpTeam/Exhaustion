using Draygo.API;
using Sandbox.ModAPI;
using System;
using System.Text;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace TSUT.Exhaustion
{
    class Hud : IDisposable
    {
        // UI Constants
        private const float DEFAULT_ICON_WIDTH = 0.03f;
        private const float DEFAULT_ICON_HEIGHT = 0.03f;
        private const float DEFAULT_BAR_WIDTH = 0.2f;
        private const float DEFAULT_BAR_HEIGHT = 0.035f;
        private const float DEFAULT_SPACING = 0.01f;
        private static readonly Vector2 DEFAULT_BASE_POS = new Vector2(-0.1f, -0.50f);
        private const float LOW_STAMINA_THRESHOLD = 0.25f;
        private const float OVERLAY_MAX_ALPHA = 0.95f;
        private const int STAMINA_LABEL_CAPACITY = 4; // For values like "100%"

        // Cached materials
        private static readonly MyStringId SQUARE_MATERIAL = MyStringId.GetOrCompute("Square");
        private static readonly MyStringId STAMINA_ICON_MATERIAL = MyStringId.GetOrCompute("StaminaIconMale");
        private static readonly MyStringId STAMINA_LOW_OVERLAY_MATERIAL = MyStringId.GetOrCompute("StaminaLowOverlay");

        private float iconWidth = DEFAULT_ICON_WIDTH;
        private float iconHeight = DEFAULT_ICON_HEIGHT;
        private float barWidth = DEFAULT_BAR_WIDTH;
        private float barHeight = DEFAULT_BAR_HEIGHT;
        private float spacing = DEFAULT_SPACING;
        private Vector2 basePos = DEFAULT_BASE_POS;

        private HudAPIv2.HUDMessage hudStaminaLabel;
        private HudAPIv2.BillBoardHUDMessage hudStaminaFill;
        private HudAPIv2.BillBoardHUDMessage hudStaminaLowOverlay;
        private HudAPIv2.BillBoardHUDMessage hudIcon;
        private HudAPIv2.BillBoardHUDMessage hudBackground;

        private float stamina;
        private bool isDisposed;

        internal bool refreshNeeded;
        private bool lowStaminaWarned = false;
        private readonly Color noColor;
        private bool onceRendered = false;
        private bool lastHudState = true;

        public Hud()
        {
            if (MyAPIGateway.Session == null)
                throw new InvalidOperationException("Session is not available");

            var viewportSize = MyAPIGateway.Session.Camera.ViewportSize;
            float aspectRatio = viewportSize.X / viewportSize.Y;

            iconWidth = DEFAULT_ICON_WIDTH / aspectRatio;
            stamina = 0;
            refreshNeeded = false;
            noColor = new Color(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        public void ReactOnSettings(Config config)
        {
            if (config?.ShowSeparateIndicator == true && onceRendered && hudStaminaLabel == null)
            {
                RenderNew();
            }
            if (config?.ShowSeparateIndicator == false && hudStaminaLabel != null)
            {
                ClearInterface();
            }
        }

        private void ClearInterface()
        {
            hudStaminaLowOverlay?.DeleteMessage();
            hudStaminaFill?.DeleteMessage();
            hudIcon?.DeleteMessage();
            hudBackground?.DeleteMessage();
            hudStaminaLabel?.DeleteMessage();

            hudStaminaLabel = null;
            hudStaminaFill = null;
            hudStaminaLowOverlay = null;
            hudIcon = null;
            hudBackground = null;
        }

        public void Update(float num)
        {
            if (num < 0 || num > 1)
                num = 1;

            stamina = num;
            refreshNeeded = true;
        }

        public void Refresh()
        {
            if (MyAPIGateway.Session == null) return;

            if (hudStaminaFill != null && hudStaminaLabel != null)
            {
                UpdateNew();
                refreshNeeded = false;
            }

            bool currentHudState = MyAPIGateway.Session.Config.HudState != 0;
            if (currentHudState != lastHudState)
            {
                if (currentHudState)
                {
                    RenderNew();
                }
                else
                {
                    ClearInterface();
                }
                lastHudState = currentHudState;
            }
        }

        private void UpdateNew()
        {
            UpdateStaminaBar();
            UpdateStaminaLabel();
            UpdateLowStaminaWarning();
            UpdateLowStaminaOverlay();
        }

        private void UpdateStaminaBar()
        {
            float fillWidth = barWidth * stamina;
            Vector2 barPos = basePos + new Vector2(iconWidth + barWidth / 2f, 0f);
            Vector2 adjustedFillPos = barPos + new Vector2((fillWidth - barWidth) / 2f, 0f);

            hudStaminaFill.Origin = adjustedFillPos;
            hudStaminaFill.Width = fillWidth;
        }

        private void UpdateStaminaLabel()
        {
            hudStaminaLabel.Message.Clear();
            hudStaminaLabel.Message.AppendFormat($"{(int)(stamina * 100)}");
        }

        private void UpdateLowStaminaWarning()
        {
            if (stamina < LOW_STAMINA_THRESHOLD && !lowStaminaWarned)
            {
                MyAPIGateway.Utilities.ShowNotification("You're critically tired!", 5000, "Red");
                hudStaminaFill.BillBoardColor = Color.Red;
                lowStaminaWarned = true;
            }
            if (stamina >= LOW_STAMINA_THRESHOLD)
            {
                lowStaminaWarned = false;
                hudStaminaFill.BillBoardColor = Color.White;
            }
        }

        private void UpdateLowStaminaOverlay()
        {
            if (stamina < LOW_STAMINA_THRESHOLD)
            {
                float alpha = CalculateSigmoidAlpha(stamina);
                hudStaminaLowOverlay.BillBoardColor = new Color(new Vector4(1.0f, 1.0f, 1.0f, alpha));
            }
            else
            {
                hudStaminaLowOverlay.BillBoardColor = noColor;
            }
        }

        private static float CalculateSigmoidAlpha(float stamina)
        {
            // Sigmoid - so it reaches near-full saturation earlier than stamina gets to zero
            float alpha = 1.0f - 1.0f / (1.0f + (float)Math.Exp(-50.0f * stamina + 6.25f));
            return Math.Min(OVERLAY_MAX_ALPHA, Math.Max(0.0f, alpha));
        }

        public void RenderNew()
        {
            onceRendered = true;

            Vector2 barPos = basePos + new Vector2(iconWidth + barWidth / 2f, 0f);
            Vector2 labelPos = barPos + new Vector2(barWidth + spacing + -0.1f, 0.013f);

            RenderBackground(barPos);
            RenderStaminaBar(barPos);
            RenderIcon();
            RenderLabel(labelPos);
            RenderLowStaminaOverlay();
        }

        private void RenderBackground(Vector2 barPos)
        {
            hudBackground = new HudAPIv2.BillBoardHUDMessage(
                Material: SQUARE_MATERIAL,
                Origin: barPos,
                BillBoardColor: Color.Black * 0.5f,
                Width: barWidth,
                Height: barHeight,
                HideHud: false,
                Shadowing: true,
                Blend: BlendTypeEnum.PostPP);
        }

        private void RenderStaminaBar(Vector2 barPos)
        {
            float fillWidth = barWidth * stamina;
            Vector2 adjustedFillPos = barPos + new Vector2((fillWidth - barWidth) / 2f, 0f);

            hudStaminaFill = new HudAPIv2.BillBoardHUDMessage(
                Material: SQUARE_MATERIAL,
                Origin: adjustedFillPos,
                BillBoardColor: Color.White,
                Width: fillWidth,
                Height: barHeight,
                HideHud: false,
                Shadowing: true,
                Blend: BlendTypeEnum.PostPP);
        }

        private void RenderIcon()
        {
            hudIcon = new HudAPIv2.BillBoardHUDMessage(
                Material: STAMINA_ICON_MATERIAL,
                Origin: basePos,
                BillBoardColor: Color.White,
                Width: iconWidth,
                Height: iconHeight,
                HideHud: false,
                Shadowing: true,
                Blend: BlendTypeEnum.PostPP);
        }

        private void RenderLabel(Vector2 labelPos)
        {
            hudStaminaLabel = new HudAPIv2.HUDMessage
            {
                Message = new StringBuilder(STAMINA_LABEL_CAPACITY),
                Origin = labelPos,
                Blend = BlendTypeEnum.PostPP,
                ShadowColor = Color.Black
            };
        }

        private void RenderLowStaminaOverlay()
        {
            hudStaminaLowOverlay = new HudAPIv2.BillBoardHUDMessage(
                Material: STAMINA_LOW_OVERLAY_MATERIAL,
                Origin: new Vector2D(0.0d, 0.0d),
                BillBoardColor: noColor,
                Width: 2.0f,
                Height: 2.0f,
                Blend: BlendTypeEnum.Standard);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                ClearInterface();
                isDisposed = true;
            }
        }
    }
}
