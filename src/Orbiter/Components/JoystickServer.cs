using Orbiter.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    // TODO
    public enum Button1
    {
        Select = 1,
        Start = 2,
    }

    public class JoystickServer : Component
    {
        public const short Port = 4263;

        private readonly JoystickInfo[] states = new JoystickInfo[256];
        private readonly object stateLock = new object();
        private StreamSocketListener socket;

        public override async void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            this.socket = new StreamSocketListener();
            this.socket.ConnectionReceived += this.OnConnectionReceived;
            await this.socket.BindServiceNameAsync(Port.ToString());
        }

        private async void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.WriteLine($"Connected to {args.Socket.Information.RemoteAddress}");

            var stream = args.Socket.InputStream.AsStreamForRead();

            var buffer = new byte[Marshal.SizeOf<JoystickInfo>()];

            try
            {
                while (true)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    var joystickInfo = ByteHelper.FromBytes<JoystickInfo>(buffer);
                    lock (this.stateLock)
                    {
                        this.states[joystickInfo.ControllerId] = joystickInfo;
                    }
                }
            }
            catch (Exception ex)
            {
            Debug.WriteLine($"Connection to {args.Socket.Information.RemoteAddress} lost");
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
