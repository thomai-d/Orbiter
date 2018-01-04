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
using Urho.Physics;
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
        private RigidBody rigidBody;
        private CollisionShape collisionShape;

        public float Mass { get; private set; }

        public void Place(Vector3 position, float diameter)
        {
            this.Node.SetScale(diameter);
            this.Mass = Physics.PlanetDiameterToMass(diameter);
            this.Node.SetWorldPosition(position);

            var earth = this.Node.CreateComponent<Sphere>();
            this.Node.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: -4, deltaAngleZ: 0)));

            this.rigidBody = this.Node.CreateComponent<RigidBody>();
            this.rigidBody.LinearDamping = 0.1f;
            this.rigidBody.AngularDamping = 0.1f;
            this.rigidBody.Mass = 0;

            this.collisionShape = this.Node.CreateComponent<CollisionShape>();
            this.collisionShape.SetSphere(1.0f, Vector3.Zero, Quaternion.Identity);

            switch (new Random().Next(3))
            {
                case 0:
                    earth.Material = Material.FromImage("Textures/Earth.jpg");
                    break;

                case 1:
                    earth.Material = Material.FromImage("Textures/Moon.jpg");
                    break;

                case 2:
                    earth.Material = Material.FromImage("Textures/Mars.jpg");
                    break;
            }
        }
    }
}
