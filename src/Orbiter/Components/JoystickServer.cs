using Orbiter.Helpers;
using System;
using System.Runtime.InteropServices;
using Urho;
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

        public bool IsButtonDown(Button0 b)
        {
            if (b == Button0.A) return (this.Buttons0 & (int)b) > 0;
            if (b == Button0.B) return (this.Buttons0 & (int)b) > 0;
            if (b == Button0.X) return (this.Buttons0 & (int)b) > 0;
            if (b == Button0.Y) return (this.Buttons0 & (int)b) > 0;
            if (b == Button0.L) return (this.Buttons0 & (int)b) > 0;
            if (b == Button0.R) return (this.Buttons0 & (int)b) > 0;
            return false;
        }

        public bool IsButtonDown(Button1 b)
        {
            if (b == Button1.Select) return (this.Buttons1 & (int)b) > 0;
            if (b == Button1.Start) return (this.Buttons1 & (int)b) > 0;
            return false;
        }
    }

    public enum Button0
    {
        X = 1,
        A = 2,
        B = 4,
        Y = 8,
        L = 16,
        R = 64,
    }

    public enum Button1
    {
        Select = 1,
        Start = 2,
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
