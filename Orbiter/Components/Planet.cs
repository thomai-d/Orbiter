using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Shapes;

namespace Orbiter.Components
{
    public class Planet : Component
    {
        private float size = 0.3f;

        public float Size
        {
            get => this.size;
            set
            {
                this.size = value;
                this.Node.SetScale(value);
            }
        }

        public Vector3 Position
        {
            get => this.Node.WorldPosition;
            set
            {
                this.Node.SetWorldPosition(value);
            }
        }

        public void Initialize(PlanetType type)
        {
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
            }
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
        }
    }

    public enum PlanetType
    {
        Earth,
        Moon
    }
}
