using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace Orbiter.Helpers
{
    public static class Physics
    {
        public static float PlanetDiameterToMass(float diameter)
        {
            return Convert.ToSingle(Math.PI * Math.Pow(diameter * Constants.PlanetGravityBoost, 3) / 6f);
        }

        public static Vector3 Gravity(Vector3 pos1, Vector3 pos2, float mass1, float mass2)
        {
            var distance = Vector3.Distance(pos1, pos2);
            var gravity = Convert.ToSingle((mass1 * mass2) / Math.Pow(distance, 2));
            var displace = (pos2 - pos1);
            displace.Normalize();
            return displace * (float)gravity;
        }

        public static float Doppler(Vector3 camera, Vector3 objPos, Vector3 objVel)
        {
            var relPos = objPos - camera;
            var delta = relPos.LengthFast - (relPos + objVel).LengthFast;
            return Convert.ToSingle(Math.Min(Math.Max(Constants.DopplerEffectMin, delta), Constants.DopplerEffectMax));
        }
    }
}
