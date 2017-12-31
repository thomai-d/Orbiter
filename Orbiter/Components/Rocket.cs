using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;
using Urho.Physics;
using Urho.Shapes;

namespace Orbiter.Components
{
    public class Rocket : Component
    {
        public Rocket()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);
            
            // TODO Mesh
            var temp = this.Node.CreateComponent<Sphere>();
            //temp.SetMaterial(Material.FromColor(Color.Yellow));
            temp.SetMaterial(Material.FromImage("Textures\\Moon.jpg"));
            this.Node.SetScale(0.1f);
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
        }
    }
}
