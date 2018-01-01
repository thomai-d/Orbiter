using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;
using Urho.Physics;
using Urho.Resources;
using Urho.Shapes;

namespace Orbiter.Components
{
    public class Rocket : Component
    {
        private Node cameraNode;

        public Rocket()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            var planeModel = this.Node.CreateComponent<StaticModel>();
            // TODO Naming, Sound => Sounds, Material
            planeModel.Model = this.Application.ResourceCache.GetModel("Models\\Cube.mdl");
            this.Node.Rotate(new Quaternion(0, 180, 0), TransformSpace.World);
            this.Node.SetScale(0.01f);

            var soundComponent = this.Node.CreateComponent<SoundSource3D>();
            var sound = this.Application.ResourceCache.GetSound("Sound\\Rocket.wav");
            sound.Looped = true;
            soundComponent.Play(sound);
            soundComponent.Gain = 0.1f;
            soundComponent.SetDistanceAttenuation(0.0f, 1.5f, 1.0f);
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
        }
    }
}
