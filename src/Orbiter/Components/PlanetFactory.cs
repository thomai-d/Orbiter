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
    /// <summary>
    /// +--Scene
    ///    +---PlanetFactory (Component)
    ///    +---"Planets" (Node)
    ///        +---"Planet" (Node)
    ///            +---Planet (Component)
    ///        +---"Planet" (Node)
    ///            +---Planet (Component)
    /// </summary>
    public class PlanetFactory : Component, IFocusElement
    {
        private float distance = Constants.PlanetPlaceDefaultDistance;

        private Node cameraNode;
        private Node planetsRoot;
        private Node tempPlanetNode;
        private SoundSource soundSource;
        private FocusManager focusManager;
        private MovingAverage averageLocation = new MovingAverage(25);

        public PlanetFactory()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            if (this.Node != this.Scene)
                throw new InvalidOperationException("PlanetFactory should be attached to the scene");

            this.cameraNode = this.Scene.GetChild("MainCamera", true) 
                ?? throw new InvalidOperationException("'MainCamera' not found");

            this.focusManager = this.Scene.GetComponent<FocusManager>();
            this.soundSource = this.Node.CreateComponent<SoundSource>();
            this.planetsRoot = this.Node.CreateChild("Planets");
        }

        public void AddNewPlanet()
        {
            this.tempPlanetNode = this.planetsRoot.CreateChild("Planet");
            var planet = this.tempPlanetNode.CreateComponent<Planet>();
            planet.Place(new Vector3(0, 0, this.distance), Constants.PlanetDefaultDiameter);

            this.focusManager.SetFocus(this);

            this.averageLocation.Reset();
        }

        public void PlacePlanet()
        {
            this.tempPlanetNode = null;
            this.soundSource.Play(Application.ResourceCache.GetSound("Sounds\\Create.wav"));
            this.focusManager.SetFocus(null);
        }

        public void RemovePlanets()
        {
            this.planetsRoot.RemoveAllChildren();
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            if (this.tempPlanetNode == null)
                return;
            
            var rotation = this.cameraNode.Rotation;
            var headPosition = this.cameraNode.WorldPosition;
            averageLocation.AddSample(headPosition + (rotation * new Vector3(0, 0, this.distance)));
            this.tempPlanetNode.SetWorldPosition(averageLocation.Average);
            this.tempPlanetNode.SetWorldRotation(rotation);
        }

        public IEnumerable<Node> PlanetNodes => this.planetsRoot.Children;

        public MenuItem[] ContextMenu
        {
            get
            {
                return new[]
                {
                    new MenuItem("Place", this.PlacePlanet, "Place")
                };
            }
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
            if (this.tempPlanetNode == null)
                return;

            var dist = distance + relCameraDiff.Z * Constants.PlanetManipulateBoostFactor;
            this.distance = Math.Min(Math.Max(Constants.PlanetPlaceMinDistance, dist), Constants.PlanetPlaceMaxDistance);
        }
    }
}
