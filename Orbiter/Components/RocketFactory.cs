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
    /// Scene
    ///   +---RocketFactoryNode
    ///         +---RocketFactory (Component)
    ///         +---RocketNode
    ///               +---Rocket (Component)
    ///         +---RocketNode
    ///               +---Rocket (Component)
    /// </summary>
    public class RocketFactory : Component
    {
        private readonly List<Node> rocketNodes = new List<Node>();

        private SoundSource soundSource;
        private Node cameraNode;

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            this.cameraNode = this.Scene.GetChild("MainCamera", true);
            if (this.cameraNode == null)
                throw new InvalidOperationException("'MainCamera' not found");

            this.soundSource = this.Node.CreateComponent<SoundSource>();
            this.soundSource.Gain = 1.0f;
        }

        public void Fire()
        {
            this.soundSource.Play(Application.ResourceCache.GetSound("Sound\\Arrow1.wav"));

            var rocketNode = this.Node.CreateChild();
            rocketNode.SetWorldPosition(this.cameraNode.WorldPosition + this.cameraNode.Rotation * new Vector3(0, 0, 0.5f));
            rocketNode.SetWorldRotation(this.cameraNode.Rotation);
            this.rocketNodes.Add(rocketNode);

            var rocket = rocketNode.CreateComponent<Rocket>();
            var rigidBody = rocketNode.CreateComponent<RigidBody>();
            rigidBody.Mass = 1.0f;
            rigidBody.LinearRestThreshold = 0.001f;
            rigidBody.SetLinearVelocity(this.cameraNode.Rotation * new Vector3(0, 0, 3.0f));
        }
    }
}
