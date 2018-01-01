using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;
using Urho.Gui;
using Urho.Physics;
using Urho.Resources;
using Urho.Shapes;

namespace Orbiter.Components
{
    public class Rocket : Component
    {
        private PlanetFactory planetFactory;
        private JoystickServer joystickServer;
        private Node cameraNode;
        private RigidBody rigidBody;
        private Text3D text3D;

        public Rocket()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            this.planetFactory = this.Scene.GetComponent<PlanetFactory>()
                ?? throw new InvalidOperationException("'PlanetFactory' not found");

            // TODO Joystick input control? + TAP with button
            this.joystickServer = this.Scene.GetComponent<JoystickServer>()
                ?? throw new InvalidOperationException("'JoystickServer' not found");

            this.cameraNode = this.Scene.GetChild("MainCamera", false) 
                ?? throw new InvalidOperationException("'MainCamera' not found");

            var planeModel = this.Node.CreateComponent<StaticModel>();
            // TODO Naming, Sound => Sounds, Material
            planeModel.Model = this.Application.ResourceCache.GetModel("Models\\Cube.mdl");
            this.Node.Rotate(new Quaternion(0, 180, 0), TransformSpace.World);
            this.Node.SetScale(0.01f);

            this.rigidBody = this.Node.CreateComponent<RigidBody>();
            this.rigidBody.Mass = 1.0f;
            this.rigidBody.LinearRestThreshold = 0.001f;

            // TODO Constant
            rigidBody.SetLinearVelocity(this.cameraNode.Rotation * new Vector3(0, 0, 0.5f));

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

            this.ApplyDopplerEffect();

            var newGravity = Vector3.Zero;
            foreach (var planetNode in this.planetFactory.PlanetNodes)
            {
                var distance = Vector3.Distance(this.Node.WorldPosition, planetNode.WorldPosition);
                var force = (0.1f) / Math.Pow(distance, 2);
                var displace = (planetNode.WorldPosition - this.Node.WorldPosition);
                displace.Normalize();
                newGravity += displace * (float)force;
            }

            // TODO Understand quaternion vector + what is slerp, what is euler angles

            // TODO time in rotation
            var joyState = this.joystickServer.GetJoystick(0);
            this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -joyState.Y), TransformSpace.Local);
            this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, -joyState.X), TransformSpace.Local);
            if (joyState.IsButtonDown(Button.L)) this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, -1.0f), TransformSpace.Local);
            if (joyState.IsButtonDown(Button.R)) this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, 1.0f), TransformSpace.Local);

            // TODO Acceleration + Mass as Property + Units
            if (joyState.IsButtonDown(Button.B))
            {
                newGravity += (this.Node.WorldRotation * Vector3.Forward) * 0.5f;
            }
            if (joyState.IsButtonDown(Button.Y))
            {
                newGravity -= (this.Node.WorldRotation * Vector3.Forward) * 0.5f;
            }
            if (joyState.IsButtonDown(Button.X))
            {
                newGravity = Vector3.Zero;
                rigidBody.SetLinearVelocity(Vector3.Zero);
            }

            // TODO v = f / m

            rigidBody.GravityOverride = newGravity;
        }

        private void ApplyDopplerEffect()
        {
            var o = this.Node.WorldPosition;
            var c = this.Node.WorldPosition;
            var v = rigidBody.LinearVelocity;
            var delta = (o - c).LengthFast - (o - c + v).LengthFast;
            this.Node.GetComponent<SoundSource3D>().Frequency = 44100f * (1f + delta);
        }
    }
}
