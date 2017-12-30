using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Gui;

namespace Orbiter.Services
{
    public interface IGridService
    {
        void Initialize(OrbiterApplication app);

        void OnUpdate();

        bool GridVisibility { get; set; }
    }

    public class GridService : IGridService
    {
        private readonly List<Node> uiTextNodes = new List<Node>();

        private bool isVisible = false;

        private OrbiterApplication app;

        public void Initialize(OrbiterApplication app)
        {
            this.app = app;
        }

        public void OnUpdate()
        {
            foreach (var textNode in this.uiTextNodes)
                textNode.LookAt(2 * textNode.WorldPosition - this.app.LeftCamera.Node.WorldPosition, Vector3.UnitY, TransformSpace.World);
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
                    foreach (var c in this.uiTextNodes)
                        this.app.Scene.RemoveChild(c);

                    this.uiTextNodes.Clear();
                }

                this.isVisible = value;
            }
        }

        private void AddText(string text, float x, float y, float z)
        {
            var position = new Vector3(x, y, z);
            var textNode = this.app.Scene.CreateChild();
            var text3D = textNode.CreateComponent<Text3D>();
            text3D.HorizontalAlignment = HorizontalAlignment.Center;
            text3D.VerticalAlignment = VerticalAlignment.Top;
            text3D.ViewMask = 0x80000000; //hide from raycasts
            text3D.Text = text;
            text3D.SetFont(CoreAssets.Fonts.AnonymousPro, 28);
            text3D.SetColor(Color.White);
            textNode.Translate(position);
            textNode.SetScale(0.1f);
            this.uiTextNodes.Add(textNode);
        }
    }
}
