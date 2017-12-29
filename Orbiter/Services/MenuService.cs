using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Gui;

namespace Orbiter.Services
{
    public interface IMenuService
    {
        MenuItem MainMenu { get; set; }

        void Initialize(OrbiterApplication app);

        void ShowMenu(MenuItem menu);

        void Update(Vector3 headPosition, Quaternion rotation);
    }

    public class MenuService : IMenuService
    {
        private OrbiterApplication app;
        private Node menuRoot;
        private MenuItem menuItem;

        public MenuService()
        {
        }

        public MenuItem MainMenu { get; set; }

        public void Initialize(OrbiterApplication app)
        {
            this.app = app;
            this.SetupDefaultVoiceCommand();
        }

        public void ShowMenu(MenuItem menu)
        {
            const float voffset = 0.05f;

            this.menuItem = menu;
            this.menuRoot = this.app.Scene.CreateChild();
            var top = 0 - menu.SubItems.Length / 2 * voffset;

            var commands = new Dictionary<string, Action>();

            int n = 0;
            foreach (var item in menu.SubItems.Reverse())
            {
                this.AddItem(top + ++n * voffset, item, n);

                if (!string.IsNullOrEmpty(item.VoiceCommand))
                    commands.Add(item.VoiceCommand, () => this.OnItemSelected(item));
            }

            this.app.RegisterCortanaCommands(commands);
        }

        public void Update(Vector3 headPosition, Quaternion rotation)
        {
            if (this.menuRoot == null)
                return;

            this.menuRoot.SetWorldPosition(headPosition + (rotation * new Vector3(0, 0, 1)));
            this.menuRoot.SetWorldRotation(rotation);
        }

        private void OnItemSelected(MenuItem item)
        {
            item.Execute();

            this.app.Scene.RemoveChild(this.menuRoot);
            this.menuRoot = null;

            if (!item.HasSubItems)
            {
                this.SetupDefaultVoiceCommand();
                return;
            }

            this.ShowMenu(item);
        }

        private void AddItem(float offset, MenuItem item, int id)
        {
            var text = new Text3D();
            var position = new Vector3(0, offset, 0);
            var textNode = this.menuRoot.CreateChild();
            textNode.Name = id.ToString();

            var text3D = textNode.CreateComponent<Text3D>();
            text3D.HorizontalAlignment = HorizontalAlignment.Center;
            text3D.VerticalAlignment = VerticalAlignment.Center;
            text3D.ViewMask = 0x80000000; //hide from raycasts
            text3D.Text = item.Title;
            text3D.SetFont(CoreAssets.Fonts.AnonymousPro, 30);
            text3D.SetColor(Color.White);
            textNode.Translate(position);
            textNode.SetScale(0.1f);
            this.menuRoot.AddChild(textNode);
        }

        private void OnMainMenu()
        {
            if (this.MainMenu == null)
                return;

            this.ShowMenu(this.MainMenu);
        }

        private void SetupDefaultVoiceCommand()
        {
            var cmds = new Dictionary<string, Action>
            {
                { "Hey", this.OnMainMenu }
            };

            this.app.RegisterCortanaCommands(cmds);
        }
    }
}

