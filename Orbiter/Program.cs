using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.SharpReality;
using Urho.Shapes;
using Urho.Resources;

namespace Orbiter
{
    internal class Program
    {
        [MTAThread]
        static void Main()
        {
            var appViewSource = new UrhoAppViewSource<HelloWorldApplication>(new ApplicationOptions("Data"));
            appViewSource.UrhoAppViewCreated += OnViewCreated;
            CoreApplication.Run(appViewSource);
        }

        static void OnViewCreated(UrhoAppView view)
        {
            view.WindowIsSet += View_WindowIsSet;
        }

        static void View_WindowIsSet(Windows.UI.Core.CoreWindow coreWindow)
        {
            // you can subscribe to CoreWindow events here
        }
    }

    public class HelloWorldApplication : StereoApplication
    {
        Node earthNode;

        public HelloWorldApplication(ApplicationOptions opts) : base(opts) { }

        protected override async void Start()
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
            await TextToSpeech("Hello world from UrhoSharp!");

            // More advanced samples can be found here:
            // https://github.com/xamarin/urho-samples/tree/master/HoloLens
        }

        // For HL optical stabilization (optional)
        public override Vector3 FocusWorldPoint => earthNode.WorldPosition;

        //Handle input:

        Vector3 earthPosBeforeManipulations;
        public override void OnGestureManipulationStarted() => earthPosBeforeManipulations = earthNode.Position;
        public override void OnGestureManipulationUpdated(Vector3 relativeHandPosition) =>
            earthNode.Position = relativeHandPosition + earthPosBeforeManipulations;

        public override void OnGestureTapped() { }
        public override void OnGestureDoubleTapped() { }
    }
}