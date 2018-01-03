using Orbiter.Helpers;
using System;
using System.Linq;
using Urho;
using Urho.Audio;
using Urho.Physics;

namespace Orbiter.Components
{
    public class Rocket : Component
    {
        private Node cameraNode;
        private RigidBody rigidBody;
        private PlanetFactory planetFactory;
        private JoystickServer joystickServer;
        private SoundSource3D soundComponent;
        private float soundBaseFrequency;

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
            planeModel.Model = this.Application.ResourceCache.GetModel("Models\\Cube.mdl");
            this.Node.SetScale(0.01f);

            this.rigidBody = this.Node.CreateComponent<RigidBody>();
            this.rigidBody.Mass = Constants.RocketDefaultMass;
            this.rigidBody.LinearRestThreshold = 0.001f;

            rigidBody.SetLinearVelocity(this.cameraNode.Rotation * Constants.RocketLaunchVelocity);

            this.soundComponent = this.Node.CreateComponent<SoundSource3D>();
            var sound = this.Application.ResourceCache.GetSound("Sound\\Rocket.wav");
            sound.Looped = true;
            this.soundComponent.Play(sound);
            this.soundComponent.Gain = 0.1f;
            this.soundComponent.SetDistanceAttenuation(0.0f, 1.5f, 1.0f);
            this.soundBaseFrequency = this.soundComponent.Frequency;
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            this.ApplyDopplerEffect();

            var newGravity = Vector3.Zero;
            foreach (var planetNode in this.planetFactory.PlanetNodes)
            {
                newGravity += Physics.Gravity(this.Node.WorldPosition, planetNode.WorldPosition, 
                    this.rigidBody.Mass, planetNode.GetComponent<Planet>().Mass) / this.rigidBody.Mass;
            }

            // TODO time in rotation
            var joyState = this.joystickServer.GetJoystick(0);
            this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -joyState.Y), TransformSpace.Local);
            this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, -joyState.X), TransformSpace.Local);
            if (joyState.IsButtonDown(Button.L)) this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, -1.0f), TransformSpace.Local);
            if (joyState.IsButtonDown(Button.R)) this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, 1.0f), TransformSpace.Local);

            if (joyState.IsButtonDown(Button.B))
                newGravity += (this.Node.WorldRotation * Vector3.Forward) * 0.5f;

            if (joyState.IsButtonDown(Button.Y))
                newGravity -= (this.Node.WorldRotation * Vector3.Forward) * 0.5f;

            if (joyState.IsButtonDown(Button.X))
            {
                newGravity = Vector3.Zero;
                rigidBody.SetLinearVelocity(Vector3.Zero);
            }

            rigidBody.GravityOverride = newGravity;
        }

        private void ApplyDopplerEffect()
        {
            var o = this.Node.WorldPosition;
            var c = this.cameraNode.WorldPosition;
            var v = rigidBody.LinearVelocity;
            this.soundComponent.Frequency = this.soundBaseFrequency * Physics.Doppler(c, o, v);
        }
    }
}
