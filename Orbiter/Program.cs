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

        private PlanetManager planets;

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
            base.Start();

            EnableGestureManipulation = true;
            EnableGestureTapped = true;

            DirectionalLight.Brightness = 1f;
            DirectionalLight.Node.SetDirection(new Vector3(-1, 0, 0.5f));

            var planetsNode = Scene.CreateChild();
            this.planets = planetsNode.CreateComponent<PlanetManager>();
            this.planets.Initialize(this);

            this.menuService.Initialize(this);
            this.gridService.Initialize(this);

            this.menuService.MainMenu = new MenuItem("Menu", string.Empty, new MenuItem[]
            {
                new MenuItem("Add planet", () => this.planets.AddNewPlanet(), "add planet"),
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

            this.menuService.OnUpdate();
            this.gridService.OnUpdate();
        }

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
    }
}