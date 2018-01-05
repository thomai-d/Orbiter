using Orbiter.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Urho;
using Windows.Networking.Sockets;

namespace Orbiter.Components
{
    public class JoystickServer : Component
    {
        public const short Port = 4263;

        private readonly JoystickInfo[] states = new JoystickInfo[256];
        private readonly object stateLock = new object();
        private FocusManager focusManager;
        private SynchronizationContext syncContext;
        private StreamSocketListener socket;

        public override async void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            this.focusManager = this.Scene.GetComponent<FocusManager>();

            this.syncContext = SynchronizationContext.Current;

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
                        this.syncContext.Post(x => { this.focusManager.UpdateJoystickInfo(joystickInfo); }, false);
                        this.states[joystickInfo.ControllerId] = joystickInfo;
                    }
                }
            }
            catch (Exception)
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
