using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;
using Urho.Physics;
using Urho.Shapes;

namespace Orbiter.Components
{
    /// <summary>
    /// +--Scene
    ///    +---RocketFactory (Component)
    ///    +---"Rockets" (Node)
    ///        +---Node
    ///            +---Rocket (Component)
    ///        +---Node
    ///            +---Rocket (Component)
    /// </summary>
    public class RocketFactory : Component
    {
        private SoundSource soundSource;
        private Node cameraNode;
        private PlanetFactory planetFactory;
        private Node rocketsNode;

        public RocketFactory()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            if (this.Node != this.Scene)
                throw new InvalidOperationException("RocketFactory should be attached to the scene");

            this.cameraNode = this.Scene.GetChild("MainCamera", false) 
                ?? throw new InvalidOperationException("'MainCamera' not found");

            this.planetFactory = this.Scene.GetComponent<PlanetFactory>()
                ?? throw new InvalidOperationException("'PlanetFactory' not found");

            this.rocketsNode = this.Node.CreateChild("Rockets");

            this.soundSource = this.Node.CreateComponent<SoundSource>();
            this.soundSource.Gain = 1.0f;
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            foreach (var rocketNode in this.rocketsNode.Children)
            {
                var rigidBody = rocketNode.GetComponent<RigidBody>();

                var newGravity = Vector3.Zero;
                foreach (var planetNode in this.planetFactory.PlanetNodes)
                {
                    var distance = Vector3.Distance(rocketNode.WorldPosition, planetNode.WorldPosition);
                    var force = (0.1f) / Math.Pow(distance, 2);
                    var displace = (planetNode.WorldPosition - rocketNode.WorldPosition);
                    displace.Normalize();
                    newGravity += displace * (float)force;
                }

                // TODO v = f / m

                rigidBody.GravityOverride = newGravity;
            }
        }

        public void Fire()
        {
            this.soundSource.Play(Application.ResourceCache.GetSound("Sound\\Arrow1.wav"));

            var rocketNode = this.rocketsNode.CreateChild();
            rocketNode.SetWorldPosition(this.cameraNode.WorldPosition + this.cameraNode.Rotation * new Vector3(0, 0, 0.5f));
            rocketNode.SetWorldRotation(this.cameraNode.Rotation);

            var rocket = rocketNode.CreateComponent<Rocket>();
            var rigidBody = rocketNode.CreateComponent<RigidBody>();
            rigidBody.Mass = 1.0f;
            rigidBody.LinearRestThreshold = 0.001f;
            rigidBody.SetLinearVelocity(this.cameraNode.Rotation * new Vector3(0, 0, 0.3f));
        }

        public void RemoveRockets()
        {
            this.rocketsNode.RemoveAllChildren();
        }
    }
}
