using Orbiter.Helpers;
using System;
using System.Diagnostics;
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
        private SoundSource3D rocketSoundSource;
        private float soundBaseFrequency;
        private SoundSource3D engineSoundSource;
        private Sound engineSound;
        private ParticleEmitter particleEmitter;

        public Rocket()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            this.planetFactory = this.Scene.GetComponent<PlanetFactory>()
                ?? throw new InvalidOperationException("'PlanetFactory' not found");

            this.joystickServer = this.Scene.GetComponent<JoystickServer>()
                ?? throw new InvalidOperationException("'JoystickServer' not found");

            this.cameraNode = this.Scene.GetChild("MainCamera", false) 
                ?? throw new InvalidOperationException("'MainCamera' not found");

            // Geometry.
            var geometryNode = this.Node.CreateChild("Geometry");
            var planeModel = geometryNode.CreateComponent<StaticModel>();
            planeModel.Model = this.Application.ResourceCache.GetModel("Models\\Cube.mdl");
            geometryNode.SetScale(0.01f);

            // Gravity.
            this.rigidBody = this.Node.CreateComponent<RigidBody>();
            this.rigidBody.Mass = Constants.RocketDefaultMass;
            this.rigidBody.LinearRestThreshold = 0.0003f;
            rigidBody.SetLinearVelocity(this.cameraNode.Rotation * Constants.RocketLaunchVelocity);

            // Engine particle emitter.
            var particleNode = this.Node.CreateChild("RocketEngine");
            this.particleEmitter = particleNode.CreateComponent<ParticleEmitter>();
            this.particleEmitter.Effect = this.Application.ResourceCache.GetParticleEffect("Particle\\RocketEngine.xml");
            this.particleEmitter.Emitting = false;
            particleNode.Translate(new Vector3(0, 0, -0.03f));

            // Background sound.
            this.rocketSoundSource = this.Node.CreateComponent<SoundSource3D>();
            this.rocketSoundSource.SetDistanceAttenuation(0.0f, 2.5f, 1.0f);
            var sound = this.Application.ResourceCache.GetSound("Sound\\Rocket.wav");
            sound.Looped = true;
            this.rocketSoundSource.Play(sound);
            this.rocketSoundSource.Gain = 0.1f;
            this.soundBaseFrequency = this.rocketSoundSource.Frequency;

            // Engine sound.
            this.engineSoundSource = this.Node.CreateComponent<SoundSource3D>();
            this.engineSoundSource.SetDistanceAttenuation(0.0f, 2.5f, 1.0f);
            this.engineSound = this.Application.ResourceCache.GetSound("Sound\\RocketEngine.wav");
            this.engineSound.Looped = true;

            this.engineSoundSource.Play(this.engineSound);
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
            this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -joyState.Y * 2.0f), TransformSpace.Local);
            this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, -joyState.X * 2.0f), TransformSpace.Local);
            if (joyState.IsButtonDown(Button0.L)) this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, -2.0f), TransformSpace.Local);
            if (joyState.IsButtonDown(Button0.R)) this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, 2.0f), TransformSpace.Local);

            if (joyState.IsButtonDown(Button0.B))
            {
                newGravity += (this.Node.WorldRotation * Vector3.Forward) * Constants.RocketAccelerationVelocity;
                if (!this.engineSoundSource.Playing)
                {
                    this.engineSoundSource.Play(this.engineSound);
                    this.particleEmitter.Emitting = true;
                }
            }
            else if (joyState.IsButtonDown(Button0.Y))
            {
                newGravity -= (this.Node.WorldRotation * Vector3.Forward) * Constants.RocketAccelerationVelocity;
                if (!this.engineSoundSource.Playing)
                {
                    this.engineSoundSource.Play(this.engineSound);
                    this.particleEmitter.Emitting = true;
                }
            }
            else if (this.engineSoundSource.Playing)
            { 
                this.engineSoundSource.Stop();
                this.particleEmitter.Emitting = false;
            }

            if (joyState.IsButtonDown(Button0.X))
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
            this.rocketSoundSource.Frequency = this.soundBaseFrequency * Physics.Doppler(c, o, v);
            this.engineSoundSource.Frequency = this.soundBaseFrequency * Physics.Doppler(c, o, v);
        }
    }
}
