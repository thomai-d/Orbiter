using Orbiter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace Orbiter.Components
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct JoystickInfo
    {
        public byte ControllerId;
        public float X;
        public float Y;
        public byte Buttons0;
        public byte Buttons1;

        public bool IsButtonDown(Button b)
        {
            // TODO Other buttons
            if (b == Button.A) return (this.Buttons0 & (int)b) > 0;
            if (b == Button.B) return (this.Buttons0 & (int)b) > 0;
            if (b == Button.X) return (this.Buttons0 & (int)b) > 0;
            if (b == Button.Y) return (this.Buttons0 & (int)b) > 0;
            if (b == Button.L) return (this.Buttons0 & (int)b) > 0;
            if (b == Button.R) return (this.Buttons0 & (int)b) > 0;

            return false;
        }
    }

    public enum Button
    {
        X = 1,
        A = 2,
        B = 4,
        Y = 8,
        L = 16,
        R = 64,
    }

    public class JoystickServer : Component
    {
        public const short UdpPort = 4263;

        private DatagramSocket socket;

        private readonly JoystickInfo[] states = new JoystickInfo[256];
        private readonly object stateLock = new object();

        public override async void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            this.socket = new DatagramSocket();
            this.socket.MessageReceived += this.OnDataReceived;
            await this.socket.BindServiceNameAsync(UdpPort.ToString());
        }

        private void OnDataReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var buffer = new byte[Marshal.SizeOf<JoystickInfo>()];
            args.GetDataReader().ReadBytes(buffer);
            var joystickInfo = ByteHelper.FromBytes<JoystickInfo>(buffer);
            lock (this.stateLock)
            {
                this.states[joystickInfo.ControllerId] = joystickInfo;
                // TODO BUTTON4;
            }
        }

        public JoystickInfo GetJoystick(byte id)
        {
            lock (this.stateLock)
            {
                return this.states[id];
            }
        }
    }
}
