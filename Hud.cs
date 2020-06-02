using Draygo.API;
using System;
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

        public void Update(float number)
        {
            if (!HudApi.Heartbeat)
            {
                return;
            }

            if (message == null)
            {
                message = new HudAPIv2.HUDMessage(
                    Message: new StringBuilder(4),
                    Origin: new Vector2D(-0.95, 0.95),
                    Scale: 1.0,
                    HideHud: true,
                    Blend: BlendTypeEnum.PostPP
                    );
            }

            message.Message.Clear();
            message.Message.Append(Convert.ToInt32(number * 100));
        }
    }
}
