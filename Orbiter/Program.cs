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
        private readonly IMenuService menuService;
        private readonly IGridService gridService;

        Node earthNode;

        public OrbiterApplication(ApplicationOptions opts) : base(opts)
        {
            var container = new ServiceContainer();
            container.Register<IMenuService, MenuService>(new PerContainerLifetime());
            container.Register<IGridService, GridService>(new PerContainerLifetime());

            this.menuService = container.GetInstance<IMenuService>();
            this.gridService = container.GetInstance<IGridService>();
        }

        protected override void Start()
        {
            // Create a basic scene, see StereoApplication
            base.Start();

            // Enable input
            EnableGestureManipulation = true;
            EnableGestureTapped = true;

            // Create a node for the Earth
            earthNode = Scene.CreateChild();
            earthNode.Position = new Vector3(0, 0, 1.5f); //1.5m away
            earthNode.SetScale(0.3f); //D=30cm

            // Scene has a lot of pre-configured components, such as Cameras (eyes), Lights, etc.
            DirectionalLight.Brightness = 1f;
            DirectionalLight.Node.SetDirection(new Vector3(-1, 0, 0.5f));

            //Sphere is just a StaticModel component with Sphere.mdl as a Model.
            var earth = earthNode.CreateComponent<Sphere>();
            earth.Material = Material.FromImage("Textures/Earth.jpg");

            var moonNode = earthNode.CreateChild();
            moonNode.SetScale(0.27f); //27% of the Earth's size
            moonNode.Position = new Vector3(1.2f, 0, 0);

            // Same as Sphere component:
            var moon = moonNode.CreateComponent<StaticModel>();
            moon.Model = CoreAssets.Models.Sphere;
            moon.Material = Material.FromImage("Textures/Moon.jpg");

            // Run a few actions to spin the Earth, the Moon and the clouds.
            earthNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: -4, deltaAngleZ: 0)));

            this.menuService.Initialize(this);
            this.gridService.Initialize(this);

            this.menuService.MainMenu = new MenuItem("Menu", string.Empty, new MenuItem[]
            {
                new MenuItem("Next", "next", new MenuItem[]
                {
                    new MenuItem("Ok", () => { this.Say("Yes"); }, "ok"),
                }),

                new MenuItem("Toggle grid", () => { this.gridService.GridVisibility = !this.gridService.GridVisibility; }, "toggle grid"),
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

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            this.menuService.Update(LeftCamera.Node.WorldPosition, LeftCamera.Node.Rotation);
            this.gridService.OnUpdate();
        }

        // For HL optical stabilization (optional)
        public override Vector3 FocusWorldPoint => earthNode.WorldPosition;

        Vector3 earthPosBeforeManipulations;
        public override void OnGestureManipulationStarted() => earthPosBeforeManipulations = earthNode.Position;
        public override void OnGestureManipulationUpdated(Vector3 relativeHandPosition) =>
            earthNode.Position = relativeHandPosition + earthPosBeforeManipulations;

        private RayQueryResult? Raycast()
        {
            Ray cameraRay = LeftCamera.GetScreenRay(0.5f, 0.5f);
            return Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
        }

        public override void OnGestureTapped()
        {
            var ray = Raycast();
            if (!ray.HasValue)
                return;
        }

        public override void OnGestureDoubleTapped()
        {
        }
    }
}