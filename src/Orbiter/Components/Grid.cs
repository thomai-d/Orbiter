using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Gui;

namespace Orbiter.Components
{
    public class Grid : Component
    {
        private bool isVisible = false;
        private Node cameraNode;
        private Node gridRoot;

        public Grid()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            if (this.Node != this.Scene)
                throw new InvalidOperationException("PlanetFactory should be attached to the scene");

            this.cameraNode = this.Scene.GetChild("MainCamera", true) 
                ?? throw new InvalidOperationException("'MainCamera' not found");

            base.OnAttachedToNode(node);
            this.gridRoot = this.Node.CreateChild();
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            foreach (var textNode in this.gridRoot.Children)
                textNode.LookAt(2 * textNode.WorldPosition - this.cameraNode.WorldPosition, Vector3.UnitY, TransformSpace.World);
        }

        public bool GridVisibility
        {
            get => this.isVisible;

            set
            {
                if (value == this.isVisible)
                    return;

                if (value)
                {
                    for (int x = -3; x <= 3; x++)
                        for (int y = 0; y <= 2; y++)
                            for (int z = -3; z <= 3; z++)
                                this.AddText($"{x}/{y}/{z}", x, y, z);
                }
                else
                {
                    foreach (var c in this.gridRoot.Children)
                        this.gridRoot.RemoveChild(c);
                }

                this.isVisible = value;
            }
        }

        private void AddText(string text, float x, float y, float z)
        {
            var position = new Vector3(x, y, z);
            var textNode = this.gridRoot.CreateChild();
            var text3D = textNode.CreateComponent<Text3D>();
            text3D.HorizontalAlignment = HorizontalAlignment.Center;
            text3D.VerticalAlignment = VerticalAlignment.Top;
            text3D.ViewMask = 0x80000000; //hide from raycasts
            text3D.Text = text;
            text3D.SetFont(CoreAssets.Fonts.AnonymousPro, 28);
            text3D.SetColor(Color.White);
            textNode.Translate(position);
            textNode.SetScale(0.05f);
        }
    }
}
