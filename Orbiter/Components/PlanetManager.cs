using Orbiter.Helpers;
using Orbiter.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;

namespace Orbiter.Components
{
    public class PlanetManager : Component, IFocusElement
    {
        public const float ManipulateBoostFactor = 3.0f;
        public const float MinDistance = 0.5f;
        public const float DefaultDistance = 2.0f;
        public const float MaxDistance = 10.0f;

        private float distance = DefaultDistance;

        private Node tempPlanetNode;
        private OrbiterApplication app;
        private SoundSource soundSource;
        private List<Planet> planets = new List<Planet>();
        private MovingAverage averageLocation = new MovingAverage(25);

        private IFocusManager focusManager;

        public PlanetManager(IFocusManager focusManager)
        {
            this.focusManager = focusManager;

            this.ReceiveSceneUpdates = true;
        }

        public void Initialize(OrbiterApplication app)
        {
            this.app = app;

            this.soundSource = this.Node.CreateComponent<SoundSource>();
            this.soundSource.Gain = 1.0f;
        }

        public void AddNewPlanet()
        {
            this.tempPlanetNode = this.Node.CreateChild();

            // TODO Use constructor
            var planet = new Planet();
            tempPlanetNode.AddComponent(planet);


            planet.Initialize(PlanetType.Earth);
            planet.Position = new Vector3(0, 0, this.distance);
            planet.Size = 0.3f;

            this.planets.Add(planet);
            this.focusManager.SetFocus(this);

            this.averageLocation.Reset();
        }

        public void PlacePlanet()
        {
            this.tempPlanetNode = null;
            this.soundSource.Play(Application.ResourceCache.GetSound("Sound\\Create.wav"));
            this.focusManager.SetFocus(null);
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            if (this.tempPlanetNode == null)
                return;
            
            var headPosition = this.app.LeftCamera.Node.WorldPosition;
            var rotation = this.app.LeftCamera.Node.Rotation;

            averageLocation.AddSample(headPosition + (rotation * new Vector3(0, 0, this.distance)));
            this.tempPlanetNode.SetWorldPosition(averageLocation.Average);
            this.tempPlanetNode.SetWorldRotation(rotation);
        }

        public void GotFocus()
        {
        }

        public void LostFocus()
        {
        }

        public void Tap()
        {
            this.PlacePlanet();
        }

        public void Manipulate(Vector3 relGlobal, Vector3 relCamera, Vector3 relCameraDiff)
        {
            // Don't manipulate if no planet is about to be placed.
            if (this.tempPlanetNode == null)
                return;

            this.distance = Math.Min(Math.Max(MinDistance, distance + relCameraDiff.Z * ManipulateBoostFactor), MaxDistance);
        }
    }
}
