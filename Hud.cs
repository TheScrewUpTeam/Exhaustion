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
        private HudAPIv2.HUDMessage message;
        
        private int number;
        internal bool refreshNeeded;

        public Hud(HudAPIv2 api)
        {
            HudApi = api;

            number = 0;
            refreshNeeded = false;
        }

        public void Update(int num)
        {
            number = num;
            refreshNeeded = true;
        }

        public void Refresh()
        {
            if (!HudApi.Heartbeat)
            {
                return;
            }

            refreshNeeded = false;

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
            message.Message.Append(number);
        }
    }
}
