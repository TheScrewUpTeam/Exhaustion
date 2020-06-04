// By THDigi. File from:
// https://github.com/THDigi/SE-ModScript-Examples/tree/738e02fdddfbd03de4018829784b5ccb1f6cf251/Data/Scripts/Examples/Example_NetworkProtobuf

using ProtoBuf;
using Sandbox.ModAPI;
//using VRage.Utils;

namespace Keyspace.Stamina
{
    // tag numbers in ProtoInclude collide with numbers from ProtoMember in the same class, therefore they must be unique.
    [ProtoInclude(1000, typeof(StatsPacket))]
    [ProtoContract]
    public abstract class PacketBase
    {
        // this field's value will be sent if it's not the default value.
        // to define a default value you must use the [DefaultValue(...)] attribute.
        [ProtoMember(1)]
        public readonly ulong SenderId;

        public PacketBase()
        {
            SenderId = MyAPIGateway.Multiplayer.MyId;
        }

        /// <summary>
        /// Called when this packet is received on this machine.
        /// </summary>
        /// <returns>Return true if you want the packet to be sent to other clients (only works server side)</returns>
        public abstract bool Received();
    }

    // An example packet extending another packet.
    // Note that it must be ProtoIncluded in PacketBase for it to work.
    [ProtoContract]
    public class StatsPacket : PacketBase
    {
        public StatsPacket() { } // Empty constructor required for deserialization

        // tag numbers in this class won't collide with tag numbers from the base class
        [ProtoMember(1)]
        public float Number;

        public StatsPacket(float number)
        {
            Number = number;
        }

        public override bool Received()
        {
            //var msg = $"StatsPacket received: Number={Number}";
            //MyLog.Default.WriteLineAndConsole(msg);
            //MyAPIGateway.Utilities.ShowNotification(msg, 3000);

            Stamina_Session.Instance.HUD?.Update(Number);

            return false;
        }
    }
}
