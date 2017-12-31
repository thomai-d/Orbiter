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
    public class Rocket : Component
    {
        private RigidBody rigidBody;
        private SoundSource soundSource;

        public Rocket()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            // TODO
            var temp = this.Node.CreateComponent<Sphere>();
            temp.SetMaterial(Material.FromColor(Color.Yellow));

            this.soundSource = this.Node.CreateComponent<SoundSource>();
            this.soundSource.Gain = 1.0f;

            this.Node.SetScale(0.1f);

            this.rigidBody = this.Node.CreateComponent<RigidBody>();
            this.rigidBody.Mass = 1.0f;
            this.rigidBody.LinearRestThreshold = 0.001f;
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
        }

        public void Fire(Vector3 start, Quaternion direction, float velocity)
        {
            this.Node.SetWorldPosition(start + direction * new Vector3(0, 0, 0.5f));
            this.Node.SetWorldRotation(direction);
            this.rigidBody.SetLinearVelocity(direction * new Vector3(0, 0, velocity));

            this.soundSource.Play(Application.ResourceCache.GetSound("Sound\\Arrow1.wav"));
        }
    }
}
