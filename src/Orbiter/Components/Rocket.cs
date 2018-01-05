using Orbiter.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;
using Urho.Physics;

namespace Orbiter.Components
{
    public class Rocket : Component, IFocusElement
    {
        private Node cameraNode;
        private Node geometryNode;
        private RigidBody rigidBody;
        private PlanetFactory planetFactory;
        private JoystickServer joystickServer;
        private FocusManager focusManager;
        private SoundSource3D rocketSoundSource;
        private float soundBaseFrequency;
        private SoundSource3D collisionSoundSource;
        private SoundSource3D engineSoundSource;
        private Sound engineSound;
        private ParticleEmitter engineParticleEmitter;
        private CollisionShape collisionShape;
        private ParticleEmitter collisionParticleEmitter;
        private Sound collisionSound;

        private JoystickInfo joyState = new JoystickInfo();

        private bool isCollided = false;

        public Rocket()
        {
            this.ReceiveSceneUpdates = true;
        }

        public MenuItem[] ContextMenu => new[] { new MenuItem("Destroy", () => this.OnCollision(), "destroy") };

        public async void OnCollision()
        {
            if (this.isCollided)
                return;

            this.focusManager.ReleaseFocus(this);

            // Stop animations / sounds / gravity.
            this.isCollided = true;
            this.engineParticleEmitter.Enabled = false;
            this.engineSoundSource.Stop();
            this.rocketSoundSource.Stop();
            this.rigidBody.GravityOverride = Vector3.Zero;

            // Explosion
            this.collisionParticleEmitter.Enabled = true;
            this.collisionSoundSource.Play(this.collisionSound);

            await Task.Delay(100);
            this.geometryNode.Remove();

            await Task.Delay(1000);
            this.collisionParticleEmitter.Emitting = false;

            await Task.Delay(1500);
            this.Node.Remove();
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            this.planetFactory = this.Scene.GetComponent<PlanetFactory>()
                ?? throw new InvalidOperationException("'PlanetFactory' not found");

            this.joystickServer = this.Scene.GetComponent<JoystickServer>()
                ?? throw new InvalidOperationException("'JoystickServer' not found");

            this.focusManager = this.Scene.GetComponent<FocusManager>()
                ?? throw new InvalidOperationException("'FocusManager' not found");

            this.cameraNode = this.Scene.GetChild("MainCamera", false) 
                ?? throw new InvalidOperationException("'MainCamera' not found");

            // Geometry.
            this.geometryNode = this.Node.CreateChild("Geometry");
            var planeModel = this.geometryNode.CreateComponent<StaticModel>();
            planeModel.Model = this.Application.ResourceCache.GetModel("Models\\Rocket.mdl");
            planeModel.Material = this.Application.ResourceCache.GetMaterial("Materials\\RocketMaterial.xml");

            // Gravity.
            this.rigidBody = this.Node.CreateComponent<RigidBody>();
            this.rigidBody.Mass = Constants.RocketDefaultMass;
            this.rigidBody.LinearRestThreshold = 0.0003f;
            this.rigidBody.AngularDamping = 0;
            this.rigidBody.SetAngularFactor(Vector3.Zero);
            rigidBody.SetLinearVelocity(this.cameraNode.Rotation * Constants.RocketLaunchVelocity);

            // Engine particle emitter.
            var engineParticleNode = this.Node.CreateChild("RocketEngine");
            this.engineParticleEmitter = engineParticleNode.CreateComponent<ParticleEmitter>();
            this.engineParticleEmitter.Enabled = false;
            this.engineParticleEmitter.Effect = this.Application.ResourceCache.GetParticleEffect("Particles\\RocketEngine.xml");
            engineParticleNode.Translate(new Vector3(0, 0, -0.03f));

            // Collision particles.
            var collisionParticleNode = this.Node.CreateChild("CollisionParticle");
            this.collisionParticleEmitter = collisionParticleNode.CreateComponent<ParticleEmitter>();
            this.collisionParticleEmitter.Enabled = false;
            this.collisionParticleEmitter.Effect = this.Application.ResourceCache.GetParticleEffect("Particles\\Explosion.xml");

            // Collision detection.
            this.collisionShape = this.Node.CreateComponent<CollisionShape>();
            this.collisionShape.SetCylinder(0.07f, 0.015f, Vector3.Zero, Quaternion.Identity);

            // Background sound.
            this.rocketSoundSource = this.Node.CreateComponent<SoundSource3D>();
            this.rocketSoundSource.SetDistanceAttenuation(0.0f, 2.0f, 1.0f);
            var sound = this.Application.ResourceCache.GetSound("Sounds\\Rocket.wav");
            sound.Looped = true;
            this.rocketSoundSource.Play(sound);
            this.rocketSoundSource.Gain = 0.1f;
            this.soundBaseFrequency = this.rocketSoundSource.Frequency;

            // Collision sound.
            this.collisionSoundSource = this.Node.CreateComponent<SoundSource3D>();
            this.collisionSound = this.Application.ResourceCache.GetSound("Sounds\\Collision.wav");
            this.collisionSoundSource.SetDistanceAttenuation(0.0f, 5.0f, 3.0f);

            // Engine sound.
            this.engineSoundSource = this.Node.CreateComponent<SoundSource3D>();
            this.engineSound = this.Application.ResourceCache.GetSound("Sounds\\RocketEngine.wav");
            this.engineSoundSource.SetDistanceAttenuation(0.0f, 5.0f, 1.0f);
            this.engineSound.Looped = true;
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            if (this.isCollided)
                return;

            this.ApplyDopplerEffect();

            var newGravity = Vector3.Zero;
            newGravity = this.ApplyGravity(newGravity);
            newGravity = this.ApplyJoystickInput(newGravity);
            rigidBody.GravityOverride = newGravity;
        }


        private Vector3 ApplyGravity(Vector3 newGravity)
        {
            foreach (var planetNode in this.planetFactory.PlanetNodes)
            {
                newGravity += Physics.Gravity(this.Node.WorldPosition, planetNode.WorldPosition,
                    this.rigidBody.Mass, planetNode.GetComponent<Planet>().Mass) / this.rigidBody.Mass;
            }

            return newGravity;
        }

        private Vector3 ApplyJoystickInput(Vector3 newGravity)
        {
            // TODO rotation should be dependent on time.

            this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -this.joyState.Y1 * 2.0f), TransformSpace.Local);
            this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, -this.joyState.X1 * 2.0f), TransformSpace.Local);
            this.Node.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, this.joyState.X2 * 2.0f), TransformSpace.Local);

            if (this.joyState.IsButtonDown(Button.L2))
            {
                newGravity += (this.Node.WorldRotation * Vector3.Forward) * Constants.RocketAccelerationVelocityMax;
                if (!this.engineSoundSource.Playing)
                {
                    this.engineSoundSource.Play(this.engineSound);
                    this.engineParticleEmitter.Enabled = true;
                }
            }
            else if (this.joyState.IsButtonDown(Button.R2))
            {
                newGravity += (this.Node.WorldRotation * Vector3.Forward) * Constants.RocketAccelerationVelocity;
                if (!this.engineSoundSource.Playing)
                {
                    this.engineSoundSource.Play(this.engineSound);
                    this.engineParticleEmitter.Enabled = true;
                }
            }
            else if (this.engineSoundSource.Playing)
            {
                this.engineSoundSource.Stop();
                this.engineParticleEmitter.Enabled = false;
            }

            // Look at velocity.
            if (this.joyState.IsButtonDown(Button.B2))
            {
                this.Node.LookAt(this.Node.WorldPosition + this.rigidBody.LinearVelocity, Vector3.Up, TransformSpace.World);
            }

            // Look against velocity.
            if (this.joyState.IsButtonDown(Button.B1))
            {
                this.Node.LookAt(this.Node.WorldPosition - this.rigidBody.LinearVelocity, Vector3.Up, TransformSpace.World);
            }

            // Orbiting.
            if (this.joyState.IsButtonDown(Button.B4))
            {
                var (nearestPlanet, distance) = this.planetFactory.PlanetNodes.GetMin(planet => Vector3.Distance(planet.WorldPosition, this.Node.WorldPosition));
                if (nearestPlanet != null &&  distance < Constants.MaxOrbit)
                    this.Node.LookAt(nearestPlanet.WorldPosition, Vector3.Up, TransformSpace.World);
            }

            return newGravity;
        }

        private void ApplyDopplerEffect()
        {
            var o = this.Node.WorldPosition;
            var c = this.cameraNode.WorldPosition;
            var v = rigidBody.LinearVelocity;
            var dopplerFactor = Physics.Doppler(c, o, v);
            this.rocketSoundSource.Frequency = this.soundBaseFrequency * dopplerFactor;
            this.engineSoundSource.Frequency = this.soundBaseFrequency * dopplerFactor;
        }

        public void GotFocus()
        {
        }

        public void LostFocus()
        {
        }

        public void Tap()
        {
        }

        public void Manipulate(Vector3 relGlobal, Vector3 relCamera, Vector3 relCameraDiff)
        {
        }

        public void UpdateJoystickInfo(JoystickInfo oldState, JoystickInfo newState)
        {
            if (oldState.IsButtonDown(Button.Start, newState))
            {
                this.OnCollision();
            }

            this.joyState = newState;
        }
    }
}
