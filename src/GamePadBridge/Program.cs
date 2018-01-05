using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GamePadBridge
{
    class Program
    {
        public const bool Simulate = false;
        public const string TargetIp = "192.168.0.103";
        public const short Port = 4263;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Packet
        {
            public byte ControllerId;
            public float X1;
            public float Y1;
            public float X2;
            public float Y2;
            public long ButtonFlags;
        }

        public static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Loop();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                }

                Thread.Sleep(3000);
            }
        }

        public static void Loop()
        {
            var dxinput = new DirectInput();
            var joysticks = new List<Joystick>();

            int joystickId = 0;
            foreach (var dev in dxinput.GetDevices().Where(g => g.Type == DeviceType.Joystick))
            {
                var joystick = new Joystick(dxinput, dev.InstanceGuid);

                Console.WriteLine($"Registered {dev.InstanceName} ({dev.ProductName}) as joystick {joystickId++}");

                foreach (var doi in joystick.GetObjects(ObjectDeviceType.Axis))
                {
                    joystick.GetObjectPropertiesById((int)doi.ObjectType).SetRange(-32767, 32767);
                }

                joystick.Acquire();
                joysticks.Add(joystick);
            }

            if (!joysticks.Any())
            {
                Console.WriteLine("No joystick found");
                return;
            }

            var tcpSocket = new TcpClient();
            var endPoint = new IPEndPoint(IPAddress.Parse(TargetIp), Port);
            tcpSocket.Client.NoDelay = true;
            if (!Simulate)
                tcpSocket.Connect(endPoint);
            Console.WriteLine("Connected!");

            // Initialize packets.
            var packets = new Packet[joysticks.Count];
            for(int n = 0; n < packets.Length; n++)
            {
                packets[n] = new Packet { ControllerId = (byte)n };
            }

            var stopwatch = Stopwatch.StartNew();
            var polls = 0;
            var sends = 0;
            while (true)
            {
                for(byte id = 0; id < joysticks.Count; id++)
                {
                    var joystick = joysticks[id];
                    var packet = packets[id];

                    // Read joystick.
                    joystick.Poll();
                    var state = joystick.GetCurrentState();
                    var buttons = state.GetButtons();
                    var x1 = state.X / 32767f;
                    var y1 = state.Y / 32767f;
                    var x2 = state.Z / 32767f;
                    var y2 = state.RotationZ / 32767f;
                    var b = 0L;
                    for (int n = 0; n < 64; n++)
                    {
                        if (buttons[n]) b |= (1L << n);
                    }

                    // Ignore if nothing changed.
                    if (packet.X1 == x1 && packet.Y1 == y1
                        && packet.X2 == x2 && packet.Y2 == y2
                        && packet.ButtonFlags == b)
                        continue;

                    // Apply changes.
                    packet.X1 = x1;
                    packet.Y1 = y1;
                    packet.X2 = x2;
                    packet.Y2 = y2;
                    packet.ButtonFlags = b;

                    if (Simulate)
                        Console.WriteLine($"{packet.X1:F2} {packet.Y1:F2} {packet.X2:F2} {packet.Y2:F2} - {packet.ButtonFlags}");

                    // Send packet.
                    var buffer = ByteHelper.GetBytes(packet);
                    if (!Simulate)
                        tcpSocket.Client.Send(buffer);
                    sends++;

                    packets[id] = packet;
                }

                polls++;
                if (stopwatch.Elapsed > TimeSpan.FromSeconds(1))
                {
                    stopwatch.Restart();
                    Console.WriteLine($"Polling rate: {polls}/sec, Packets sent: {sends}/sec");

                    polls = 0;
                    sends = 0;
                }

                Thread.Sleep(15);
            }
        }
    }
}
