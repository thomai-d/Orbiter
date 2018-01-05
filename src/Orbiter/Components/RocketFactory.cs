using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        }

        public void Fire()
        {
            this.soundSource.Play(Application.ResourceCache.GetSound("Sounds\\Arrow1.wav"));

            var rocketNode = this.rocketsNode.CreateChild();
            var rocket = rocketNode.CreateComponent<Rocket>();

            rocketNode.SetWorldPosition(this.cameraNode.WorldPosition + this.cameraNode.WorldRotation * Constants.RocketRelativeStartOffset);
            rocketNode.SetWorldRotation(this.cameraNode.WorldRotation);
        }

        public void RemoveRockets()
        {
            this.rocketsNode.RemoveAllChildren();
        }
    }
}
