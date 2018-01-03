using Orbiter.Helpers;
using Orbiter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Audio;
using Urho.Shapes;

namespace Orbiter.Components
{
    public enum PlanetType
    {
        Earth,
        Moon,
        Mars
    }

    public class Planet : Component
    {
        public float Mass { get; private set; }

        public void Place(PlanetType type, Vector3 position, float diameter)
        {
            this.Node.SetScale(diameter);
            this.Mass = Physics.PlanetDiameterToMass(diameter);
            this.Node.SetWorldPosition(position);

            var earth = this.Node.CreateComponent<Sphere>();
            this.Node.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: -4, deltaAngleZ: 0)));

            switch (type)
            {
                case PlanetType.Earth:
                    earth.Material = Material.FromImage("Textures/Earth.jpg");
                    break;

                case PlanetType.Moon:
                    earth.Material = Material.FromImage("Textures/Moon.jpg");
                    break;

                case PlanetType.Mars:
                    earth.Material = Material.FromImage("Textures/Mars.jpg");
                    break;
            }
        }
    }
}
