using Orbiter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace Orbiter.Components
{
    public class StayInFrontOfCamera : Component
    {
        private Node cameraNode;
        private MovingAverage averageLocation = new MovingAverage(25);

        public StayInFrontOfCamera()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            this.cameraNode = this.Scene.GetChild("MainCamera", true) 
                ?? throw new InvalidOperationException("'MainCamera' not found");
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            var rotation = this.cameraNode.Rotation;
            var headPosition = this.cameraNode.WorldPosition;
            averageLocation.AddSample(headPosition + (rotation * new Vector3(0, 0, 1.5f)));
            this.Node.SetWorldPosition(averageLocation.Average);
            this.Node.SetWorldRotation(rotation);
        }
    }
}
