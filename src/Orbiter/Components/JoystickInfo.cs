using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orbiter.Components
{
    public enum Button
    {
        B1 = 1,
        B2 = 2,
        B3 = 4,
        B4 = 8,
        L1 = 16,
        R1 = 32,
        L2 = 64,
        R2 = 128,
        Select = 256,
        Start = 512,
        LAnalog = 1024,
        RAnalog = 2048
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct JoystickInfo
    {
        public byte ControllerId;
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;
        public long ButtonFlags;

        public bool IsButtonDown(Button b)
        {
            return (this.ButtonFlags & (int)b) > 0;
        }

        public bool IsButtonDown(Button b, JoystickInfo newState)
        {
            var wasDown = this.IsButtonDown(b);
            var isDown = newState.IsButtonDown(b);
            return !wasDown && isDown;
        }
    }
}
