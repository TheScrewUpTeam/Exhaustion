using ProtoBuf;
using Sandbox.ModAPI;
using VRage.Utils;

// File from:
// https://github.com/THDigi/SE-ModScript-Examples/tree/738e02fdddfbd03de4018829784b5ccb1f6cf251/Data/Scripts/Examples/Example_NetworkProtobuf

namespace Keyspace.Stamina
{
    // tag numbers in ProtoInclude collide with numbers from ProtoMember in the same class, therefore they must be unique.
    [ProtoInclude(1000, typeof(PacketSimpleExample))]
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
    public class PacketSimpleExample : PacketBase
    {
        public PacketSimpleExample() { } // Empty constructor required for deserialization

        // tag numbers in this class won't collide with tag numbers from the base class
        [ProtoMember(1)]
        public string Text;

        [ProtoMember(2)]
        public int Number;

        public PacketSimpleExample(string text, int number)
        {
            Text = text;
            Number = number;
        }

        public override bool Received()
        {
            //var msg = $"PacketSimpleExample received: Text='{Text}'; Number={Number}";
            //MyLog.Default.WriteLineAndConsole(msg);
            //MyAPIGateway.Utilities.ShowNotification(msg, Number);

            Stamina_Session.Instance.HUD?.Update(Number);

            return false;
        }
    }
}
