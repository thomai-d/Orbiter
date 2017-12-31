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
        public const float ManipulateBoostFactor = 5.0f;
        public const float MinDistance = 0.5f;
        public const float DefaultDistance = 2.0f;
        public const float MaxDistance = 10.0f;

        private float distance = DefaultDistance;

        private Node cameraNode;
        private Node tempPlanetNode;
        private SoundSource soundSource;
        private MovingAverage averageLocation = new MovingAverage(25);

        private IFocusManager focusManager;
        private Node planetsRoot;

        public PlanetFactory(IFocusManager focusManager)
        {
            this.ReceiveSceneUpdates = true;
            this.focusManager = focusManager;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            if (this.Node != this.Scene)
                throw new InvalidOperationException("PlanetFactory should be attached to the scene");

            this.cameraNode = this.Scene.GetChild("MainCamera", true) 
                ?? throw new InvalidOperationException("'MainCamera' not found");

            this.soundSource = this.Node.CreateComponent<SoundSource>();
            this.soundSource.Gain = 1.0f;

            this.planetsRoot = this.Node.CreateChild("Planets");
        }

        public void AddNewPlanet()
        {
            this.tempPlanetNode = this.planetsRoot.CreateChild("Planet");
            var planet = this.tempPlanetNode.CreateComponent<Planet>();

            planet.Initialize(PlanetType.Earth);
            planet.Position = new Vector3(0, 0, this.distance);
            planet.Size = 0.3f;

            this.focusManager.SetFocus(this);

            this.averageLocation.Reset();
        }

        public void PlacePlanet()
        {
            this.tempPlanetNode = null;
            this.soundSource.Play(Application.ResourceCache.GetSound("Sound\\Create.wav"));
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
            
            var headPosition = this.cameraNode.WorldPosition;
            var rotation = this.cameraNode.Rotation;

            averageLocation.AddSample(headPosition + (rotation * new Vector3(0, 0, this.distance)));
            this.tempPlanetNode.SetWorldPosition(averageLocation.Average);
            this.tempPlanetNode.SetWorldRotation(rotation);
        }

        public IEnumerable<Node> PlanetNodes => this.planetsRoot.Children;

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
