using Orbiter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace Orbiter.Components
{
    public class PlanetManager : Component
    {
        public const float AddPlanetDistance = 2.0f;

        private Node tempPlanetNode;
        private OrbiterApplication app;

        private List<Planet> planets = new List<Planet>();

        public PlanetManager()
        {
            this.ReceiveSceneUpdates = true;
        }

        public void Initialize(OrbiterApplication app)
        {
            this.app = app;
        }

        public void AddNewPlanet()
        {
            this.tempPlanetNode = this.Node.CreateChild();

            var planet = this.tempPlanetNode.CreateComponent<Planet>();
            planet.Initialize(PlanetType.Earth);
            planet.Position = new Vector3(0, 0, AddPlanetDistance);
            planet.Size = 0.3f;

            this.planets.Add(planet);

            this.averageLocation.Reset();
        }

        public void PlacePlanet()
        {
            this.tempPlanetNode = null;
        }

        private MovingAverage averageLocation = new MovingAverage(30);
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            if (this.tempPlanetNode == null)
                return;
            
            var headPosition = this.app.LeftCamera.Node.WorldPosition;
            var rotation = this.app.LeftCamera.Node.Rotation;

            averageLocation.AddSample(headPosition + (rotation * new Vector3(0, 0, AddPlanetDistance)));
            this.tempPlanetNode.SetWorldPosition(averageLocation.Average);
            this.tempPlanetNode.SetWorldRotation(rotation);
        }
    }
}
