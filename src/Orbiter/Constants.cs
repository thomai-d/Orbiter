using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace Orbiter
{
    public static class Constants
    {
        /// <summary>
        /// Constant for planets by which the diameter is multiplied before calculating the mass.
        /// </summary>
        public const float PlanetGravityBoost = 2.0f;

        /// <summary>
        /// Default diameter of a newly added planet.
        /// </summary>
        public const float PlanetDefaultDiameter = 0.3f;

        /// <summary>
        /// Default mass of a rocket.
        /// </summary>
        public const float RocketDefaultMass = 0.00001f;

        /// <summary>
        /// Default rocket lauch velocity.
        /// </summary>
        public static readonly Vector3 RocketLaunchVelocity = new Vector3(0, 0, 0.5f);

        /// <summary>
        /// Doppler effect constaints.
        /// </summary>
        public const float DopplerEffectMin = 0.3f;
        public const float DopplerEffectMax = 5.0f;

        /// <summary>
        /// Rocket start position relative to camera.
        /// </summary>
        public static readonly Vector3 RocketRelativeStartOffset = new Vector3(0, 0, 0.5f);

        /// <summary>
        /// Boost factor for planet manipulation.
        /// </summary>
        public const float PlanetManipulateBoostFactor = 5.0f;

        /// <summary>
        /// Distances for planet placements.
        /// </summary>
        public const float PlanetPlaceMinDistance = 0.5f;
        public const float PlanetPlaceDefaultDistance = 2.0f;
        public const float PlanetPlaceMaxDistance = 10.0f;
    }
}
