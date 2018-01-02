using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.SharpReality;
using Urho.Shapes;
using Urho.Resources;
using Urho.Gui;
using Orbiter.Services;
using Orbiter.Components;
using System.Diagnostics;
using Urho.Physics;
using Urho.Audio;

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
        private OnScreenMenu onScreenMenu;
        private FocusManager focusManager;
        private PlanetFactory planetFactory;
        private VoiceRecognition voiceRecognition;
        private JoystickServer joystickServer;

        public OrbiterApplication(ApplicationOptions opts) : base(opts)
        {
        }

        protected override void Start()
        {
            base.Start();

            this.LeftCamera.Node.Name = "MainCamera";

            EnableGestureManipulation = true;
            EnableGestureTapped = true;

            DirectionalLight.Brightness = 1f;
            DirectionalLight.Node.SetDirection(new Vector3(-1, 0, 0.5f));

            var physics = this.Scene.GetOrCreateComponent<PhysicsWorld>();
            physics.SetGravity(new Vector3(0, 0, 0));

            this.voiceRecognition = this.Scene.CreateComponent<VoiceRecognition>();
            this.voiceRecognition.SetRegisterCallback(this.RegisterCortanaCommands);

            this.onScreenMenu = this.Scene.CreateComponent<OnScreenMenu>();

            this.joystickServer = this.Scene.CreateComponent<JoystickServer>();

            this.focusManager = this.Scene.CreateComponent<FocusManager>();
            this.focusManager.DefaultFocus = this;
            this.focusManager.SetFocus(this);

            this.planetFactory = this.Scene.CreateComponent<PlanetFactory>();

            this.rocketFactory = this.Scene.CreateComponent<RocketFactory>();

            this.grid = this.Scene.CreateComponent<Grid>();

            var listener = this.LeftCamera.Node.CreateComponent<SoundListener>();
            Audio.Listener = listener;

            var sound = this.Scene.CreateComponent<SoundSource>();
            sound.Play(this.ResourceCache.GetSound("Sound\\Startup.wav"));
        }

        public void Say(string text)
        {
            this.TextToSpeech(text);
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            this.FocusWorldPoint = Raycast()?.Node?.WorldPosition 
                ?? LeftCamera.Node.WorldPosition + LeftCamera.Node.WorldRotation * Vector3.Forward * 2;
        }

        private RayQueryResult? Raycast()
        {
            Ray cameraRay = LeftCamera.GetScreenRay(0.5f, 0.5f);
            return Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
        }

        public override void OnGestureTapped()
        {
            // Ignore taps when manipulating.
            if (this.isManipulating)
                return;

            // Check if the tap selected something.
            //var ray = Raycast();
            //if (ray.HasValue)
            //{

            //}

            // Is some object focused that may handle the tap?
            if (this.focusManager.HandleTap())
                return;
        }

        private bool isManipulating = false;

        public override void OnGestureManipulationStarted()
        {
            base.OnGestureManipulationStarted();
            this.lastManipulationVector = Vector3.Zero;
            this.cameraStartPos = this.LeftCamera.Node.WorldPosition;
        }

        private Vector3 lastManipulationVector = Vector3.Zero;
        private Vector3 cameraStartPos = Vector3.Zero;
        private RocketFactory rocketFactory;
        private Grid grid;

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
                    new MenuItem("Toggle grid", () => { this.grid.GridVisibility = !this.grid.GridVisibility; }, "toggle grid"),
                };
            }
        }

        public override void OnGestureManipulationUpdated(Vector3 relGlobalPos)
        {
            base.OnGestureManipulationUpdated(relGlobalPos);

            this.isManipulating = true;

            var cameraVector = this.LeftCamera.Node.Position - this.cameraStartPos;
            var relLocalPos = Quaternion.Invert(this.LeftCamera.Node.Rotation) * (relGlobalPos - cameraVector);

            this.focusManager.Manipulate(relGlobalPos, relLocalPos, relLocalPos - this.lastManipulationVector);
            this.lastManipulationVector = relLocalPos;
        }

        public override void OnGestureManipulationCompleted(Vector3 relativeHandPosition)
        {
            base.OnGestureManipulationCompleted(relativeHandPosition);
            isManipulating = false;
        }

        public override void OnGestureManipulationCanceled()
        {
            base.OnGestureManipulationCanceled();
            isManipulating = false;
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
    }
}