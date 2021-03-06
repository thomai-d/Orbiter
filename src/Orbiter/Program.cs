﻿using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.SharpReality;
using Urho.Shapes;
using Urho.Resources;
using Urho.Gui;
using Orbiter.Components;
using System.Diagnostics;
using Urho.Physics;
using Urho.Audio;
using System.Threading.Tasks;

namespace Orbiter
{
    internal class Program
    {
        [MTAThread]
        static void Main()
        {
            var appViewSource = new UrhoAppViewSource<OrbiterApplication>(new ApplicationOptions("Data"));
            CoreApplication.Run(appViewSource);
        }
    }

    public class OrbiterApplication : StereoApplication, IFocusElement
    {
        // Components.
        private OnScreenMenu onScreenMenu;
        private FocusManager focusManager;
        private PlanetFactory planetFactory;
        private VoiceRecognition voiceRecognition;
        private JoystickServer joystickServer;
        private RocketFactory rocketFactory;
        private Grid grid;
        private Node splashNode;

        // Variables needed for manipulation calculation.
        private Vector3 lastManipulationVector = Vector3.Zero;
        private Vector3 cameraStartPos = Vector3.Zero;

        // Objects.
        private Node environmentNode;
        private Material spatialMaterial;

        // Flags.
        private bool isDebugging = false;
        private bool isManipulationInProgress = false;

        public OrbiterApplication(ApplicationOptions opts) : base(opts)
        {
        }

        public MenuItem[] ContextMenu
        {
            get
            {
                return new[] 
                {
                    new MenuItem("Add planet", () => this.planetFactory.AddNewPlanet(), "add planet"),
                    new MenuItem("Remove planets", () => this.planetFactory.RemovePlanets(), "remove planets"),
                    new MenuItem("Start rocket", () => this.rocketFactory.Fire(), "start rocket"),
                    new MenuItem("Remove rockets", () => this.rocketFactory.RemoveRockets(), "remove rockets"),
                    new MenuItem("Toggle debug", () => { this.SetDebug(!this.isDebugging); }, "toggle debug"),
                };
            }
        }

        // Public.

        public override void OnGestureTapped()
        {
            if (this.isManipulationInProgress)
                return;

            if (this.focusManager.HandleTap())
                return;
        }

        public override void OnGestureManipulationStarted()
        {
            base.OnGestureManipulationStarted();
            this.lastManipulationVector = Vector3.Zero;
            this.cameraStartPos = this.LeftCamera.Node.WorldPosition;
        }

        public override void OnGestureManipulationUpdated(Vector3 relGlobalPos)
        {
            base.OnGestureManipulationUpdated(relGlobalPos);

            this.isManipulationInProgress = true;

            var cameraVector = this.LeftCamera.Node.Position - this.cameraStartPos;
            var relLocalPos = Quaternion.Invert(this.LeftCamera.Node.Rotation) * (relGlobalPos - cameraVector);

            this.focusManager.Manipulate(relGlobalPos, relLocalPos, relLocalPos - this.lastManipulationVector);
            this.lastManipulationVector = relLocalPos;
        }

        public override void OnGestureManipulationCompleted(Vector3 relativeHandPosition)
        {
            base.OnGestureManipulationCompleted(relativeHandPosition);
            isManipulationInProgress = false;
        }

        public override void OnGestureManipulationCanceled()
        {
            base.OnGestureManipulationCanceled();
            isManipulationInProgress = false;
        }

        public void GotFocus()
        {
        }

        public void LostFocus()
        {
        }

        public void Tap()
        {
            this.rocketFactory.Fire();
        }

        public void Manipulate(Vector3 relGlobal, Vector3 relCamera, Vector3 relCameraDiff)
        { 
        }

        public override void OnSurfaceAddedOrUpdated(SpatialMeshInfo surface, Model generatedModel)
        {
            bool isNew = false;
            StaticModel staticModel = null;
            Node node = environmentNode.GetChild(surface.SurfaceId, false);
            if (node != null)
            {
                isNew = false;
                staticModel = node.GetComponent<StaticModel>();
            }
            else
            {
                isNew = true;
                node = environmentNode.CreateChild(surface.SurfaceId);
                staticModel = node.CreateComponent<StaticModel>();
            }

            node.Position = surface.BoundsCenter;
            node.Rotation = surface.BoundsRotation;
            staticModel.Model = generatedModel;

            if (isNew)
            {
                staticModel.SetMaterial(this.spatialMaterial);
                var collisionShape = node.CreateComponent<CollisionShape>();
                collisionShape.SetTriangleMesh(generatedModel, 0, Vector3.One, Vector3.Zero, Quaternion.Identity);
                var rigidBoy = node.CreateComponent<RigidBody>();
                rigidBoy.Mass = 0;
            }
        }

        public void UpdateJoystickInfo(JoystickInfo oldState, JoystickInfo newState)
        {
            if (oldState.IsButtonDown(Components.Button.B3, newState))
                this.rocketFactory.Fire();

            if (oldState.IsButtonDown(Components.Button.B1, newState))
                this.planetFactory.AddNewPlanet();
        }

        // Protected.

        protected override async void Start()
        {
            base.Start();

            this.LeftCamera.Node.Name = "MainCamera";

            EnableGestureManipulation = true;
            EnableGestureTapped = true;

            DirectionalLight.Node.SetWorldPosition(new Vector3(0, 1.5f, 0));
            DirectionalLight.Brightness = 1f;
            DirectionalLight.Node.SetDirection(new Vector3(0, -1, 0));

            var physics = this.Scene.GetOrCreateComponent<PhysicsWorld>();
            physics.SetGravity(new Vector3(0, 0, 0));
            physics.PhysicsCollisionStart += this.OnCollisionStart;

            this.voiceRecognition = this.Scene.CreateComponent<VoiceRecognition>();
            this.voiceRecognition.SetRegisterCallback(this.RegisterCortanaCommands);

            this.onScreenMenu = this.Scene.CreateComponent<OnScreenMenu>();

            this.focusManager = this.Scene.CreateComponent<FocusManager>();
            this.focusManager.DefaultFocus = this;
            this.focusManager.SetFocus(this);

            this.joystickServer = this.Scene.CreateComponent<JoystickServer>();

            this.planetFactory = this.Scene.CreateComponent<PlanetFactory>();

            this.rocketFactory = this.Scene.CreateComponent<RocketFactory>();

            this.grid = this.Scene.CreateComponent<Grid>();

            var listener = this.LeftCamera.Node.CreateComponent<SoundListener>();
            Audio.Listener = listener;

            // TODO: Spatial mapping. Way too slow
            //this.spatialMaterial = new Material();
            //this.spatialMaterial.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);
            //this.environmentNode = this.Scene.CreateChild("Environment");
            //var spatialMappingAllowed = await StartSpatialMapping(new Vector3(3, 3, 2), 1);
            //if (!spatialMappingAllowed)
            //    throw new InvalidOperationException("SpatialMapping is not allowed");

            await ShowSplash();
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            this.FocusWorldPoint = (this.focusManager.CurrentFocus as Node)?.WorldPosition
                ?? Raycast()?.Node?.WorldPosition 
                ?? LeftCamera.Node.WorldPosition + LeftCamera.Node.WorldRotation * Vector3.Forward * 2;
        }

        // Private.

        private async Task ShowSplash()
        {
            var soundSource = this.Scene.CreateComponent<SoundSource>();

            this.splashNode = this.Scene.CreateChild("Splash");
            this.splashNode.CreateComponent<StayInFrontOfCamera>();
            var sound = this.ResourceCache.GetSound("Sounds\\Departure.wav");
            sound.Looped = true;
            sound.SetLoop(sound.DataSize / 2, sound.DataSize);
            soundSource.Gain = 0.3f;
            soundSource.Play(sound);

            var orbiterTextNode = this.splashNode.CreateChild("Text");
            var orbiterText = orbiterTextNode.CreateComponent<Text3D>();
            orbiterText.HorizontalAlignment = HorizontalAlignment.Center;
            orbiterText.VerticalAlignment = VerticalAlignment.Top;
            orbiterText.ViewMask = 0x80000000;
            orbiterText.Text = "ORBITER";
            orbiterText.SetFont(CoreAssets.Fonts.AnonymousPro, 28);
            orbiterText.SetColor(Color.White);
            orbiterTextNode.SetScale(0.3f);

            var joystickTextNode = this.splashNode.CreateChild("Text");
            joystickTextNode.Translate(new Vector3(0, -0.1f, 0));
            var joystickText = joystickTextNode.CreateComponent<Text3D>();
            joystickText.HorizontalAlignment = HorizontalAlignment.Center;
            joystickText.VerticalAlignment = VerticalAlignment.Top;
            joystickText.ViewMask = 0x80000000;
            joystickText.Text = "Waiting for joystick to connect...";
            joystickText.SetFont(CoreAssets.Fonts.AnonymousPro, 24);
            joystickText.SetColor(Color.White);
            joystickTextNode.SetScale(0.08f);

            await this.WaitForJoystick();
            this.splashNode.Remove();

            soundSource.Stop();
            soundSource.Gain = 1.0f;
            soundSource.Play(this.ResourceCache.GetSound("Sounds\\Startup.wav"));
        }

        private async Task WaitForJoystick()
        {
            while (!this.joystickServer.IsJoystickAvailable)
            {
                await Task.Delay(1000);
            }

            return;
        }

        private void OnCollisionStart(PhysicsCollisionStartEventArgs obj)
        {
            obj.NodeA.GetComponent<Rocket>()?.OnCollision();
            obj.NodeB.GetComponent<Rocket>()?.OnCollision();
        }

        private RayQueryResult? Raycast()
        {
            Ray cameraRay = LeftCamera.GetScreenRay(0.5f, 0.5f);
            return Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
        }

        private void DrawGebugGeometry(PostRenderUpdateEventArgs _)
        {
            var debugRendererComp = this.Scene.GetComponent<DebugRenderer>();
            var physicsComp = this.Scene.GetComponent<PhysicsWorld>();
            physicsComp.DrawDebugGeometry(debugRendererComp, depthTest: false);
        }

        private void SetDebug(bool newValue)
        {
            this.isDebugging = newValue;
            this.grid.GridVisibility = newValue;
            this.Scene.GetOrCreateComponent<DebugRenderer>();
            if (newValue)
                this.Engine.PostRenderUpdate += this.DrawGebugGeometry;
            else
                this.Engine.PostRenderUpdate -= this.DrawGebugGeometry;
        }
    }
}