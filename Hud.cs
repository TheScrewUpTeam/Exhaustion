using Draygo.API;
using System.Text;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Stamina.Hud
{
    class Hud
    {
        HudAPIv2 HudApi;
        HudAPIv2.HUDMessage message;

        public Hud(HudAPIv2 api)
        {
            HudApi = api;
        }

        public void Update(int number)
        {
            if (!HudApi.Heartbeat)
            {
                return;
            }

            if (message == null)
            {
                string TEXT = $"There are {number} player(s).";
                const double SCALE = 1.0;
                var position = new Vector2D(-0.95, 0.95);

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
    }
}
