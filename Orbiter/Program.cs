using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.SharpReality;
using Urho.Shapes;
using Urho.Resources;
using Urho.Gui;
using LightInject;
using Orbiter.Services;
using Orbiter.Components;
using System.Diagnostics;
using Urho.Physics;

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

    public class OrbiterApplication : StereoApplication
    {
        private readonly IFocusManager focusManager;
        private readonly ServiceContainer container;

        private OnScreenMenu mainMenu;

        private PlanetFactory planetFactory;

        public OrbiterApplication(ApplicationOptions opts) : base(opts)
        {
            // TODO Remove DI
            this.container = new ServiceContainer();
            this.container.Register<IFocusManager, FocusManager>(new PerContainerLifetime());
            this.container.Register<PlanetFactory>(new PerContainerLifetime());
            this.focusManager = this.container.GetInstance<IFocusManager>();
        }

        protected override void Start()
        {
            base.Start();

            this.LeftCamera.Node.Name = "MainCamera";

            EnableGestureManipulation = true;
            EnableGestureTapped = true;

            DirectionalLight.Brightness = 1f;
            DirectionalLight.Node.SetDirection(new Vector3(-1, 0, 0.5f));

            this.SetupMenu();

            var physics = this.Scene.GetOrCreateComponent<PhysicsWorld>();
            physics.SetGravity(new Vector3(0, -1f, 0));

            this.rocketFactory = this.Scene.CreateComponent<RocketFactory>();

            this.grid = this.Scene.CreateComponent<Grid>();

            // TODO think about DI...
            this.planetFactory = this.container.GetInstance<PlanetFactory>();
            this.Scene.AddComponent(this.planetFactory);
        }

        private void SetupMenu()
        {
            var menuNode = Scene.CreateChild();
            this.mainMenu = menuNode.CreateComponent<OnScreenMenu>();
            this.mainMenu.Initialize(this);

            this.mainMenu.MainMenu = new MenuItem("Menu", string.Empty, new MenuItem[]
            {
                new MenuItem("Add planet", () => this.planetFactory.AddNewPlanet(), "add planet"),
                new MenuItem("Toggle grid", () => { this.grid.GridVisibility = !this.grid.GridVisibility; }, "toggle grid"),
                new MenuItem("Exit", () => { this.Say("Exit"); }, "Exit")
            });
        }

        public new void RegisterCortanaCommands(Dictionary<string, Action> actions)
        {
            base.RegisterCortanaCommands(actions);
        }

        public void Say(string text)
        {
            this.TextToSpeech(text);
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

            // Do default action.
            this.rocketFactory.Fire();
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
    }
}