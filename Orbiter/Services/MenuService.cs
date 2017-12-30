using Orbiter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Gui;

namespace Orbiter.Services
{
    public interface IMenuService
    {
        MenuItem MainMenu { get; set; }

        void Initialize(OrbiterApplication app);

        Task ShowMenuAsync(MenuItem menu);

        void OnUpdate();
    }

    public class MenuService : IMenuService
    {
        private MovingAverage averageLocation = new MovingAverage(30);
        private OrbiterApplication app;
        private Node menuRoot;
        private MenuItem menuItem;

        private TaskCompletionSource<bool> showMenuTcs { get; set; }

        public MenuService()
        {
        }

        public MenuItem MainMenu { get; set; }

        public void Initialize(OrbiterApplication app)
        {
            this.app = app;
            this.SetupDefaultVoiceCommand();
        }

        public Task ShowMenuAsync(MenuItem menu)
        {
            const float voffset = 0.03f;

            this.averageLocation.Reset();

            this.menuItem = menu;
            this.menuRoot = this.app.Scene.CreateChild();
            var top = menu.SubItems.Length / 2 * voffset;

            var commands = new Dictionary<string, Action>();

            var maxWidth = 0f;
            int n = 0;
            maxWidth = Math.Max(maxWidth, this.AddItem(top, this.menuItem, isHeadline: true));

            foreach (var item in menu.SubItems)
            {
                maxWidth = Math.Max(maxWidth, this.AddItem(top - ++n * voffset, item));

                if (!string.IsNullOrEmpty(item.VoiceCommand))
                    commands.Add(item.VoiceCommand, () => this.OnItemSelected(item));
            }

            var backNode = this.menuRoot.CreateChild();
            var back = backNode.CreateComponent<StaticModel>();
            back.Model = CoreAssets.Models.Box;
            back.SetMaterial(Material.FromColor(new Color(0.2f, 0.2f, 0.2f, 0.8f)));
            backNode.Translate(new Vector3(0, -voffset/2, 0.01f));
            backNode.ScaleNode(new Vector3(maxWidth + 0.2f, voffset * (menu.SubItems.Length + 2), 0.01f));

            this.app.RegisterCortanaCommands(commands);

            if (this.showMenuTcs == null)
            {
                this.showMenuTcs = new TaskCompletionSource<bool>(null, TaskCreationOptions.RunContinuationsAsynchronously);
            }

            this.menuRoot.SetScale(0.0f);
            this.menuRoot.RunActions(new EaseElasticOut(new ScaleTo(0.5f, 1.0f)));

            return this.showMenuTcs.Task;
        }

        public void OnUpdate()
        {
            if (this.menuRoot == null)
                return;

            var headPosition = this.app.LeftCamera.Node.WorldPosition;
            var rotation = this.app.LeftCamera.Node.Rotation;

            averageLocation.AddSample(headPosition + (rotation * new Vector3(0, 0, 1)));
            this.menuRoot.SetWorldPosition(averageLocation.Average);
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
                this.showMenuTcs.SetResult(true);
                this.showMenuTcs = null;
                return;
            }

            this.ShowMenuAsync(item);
        }

        private float AddItem(float offset, MenuItem item, bool isHeadline = false)
        {
            var text = new Text3D();
            var position = new Vector3(0, offset, 0);
            var textNode = this.menuRoot.CreateChild();

            var text3D = textNode.CreateComponent<Text3D>();
            text3D.HorizontalAlignment = HorizontalAlignment.Center;
            text3D.VerticalAlignment = VerticalAlignment.Center;
            text3D.ViewMask = 0x80000000; //hide from raycasts
            text3D.Text = item.Title;
            text3D.SetFont(CoreAssets.Fonts.AnonymousPro, isHeadline ? 30 : 25);
            text3D.SetColor(Color.White);
            textNode.Translate(position);
            textNode.SetScale(0.1f);

            return text3D.BoundingBox.Size.X * 0.1f;
        }

        private void SetupDefaultVoiceCommand()
        {
            var cmds = new Dictionary<string, Action>
            {
                { "Hey", () => { if (this.MainMenu != null) this.ShowMenuAsync(this.MainMenu); } }
            };

            this.app.RegisterCortanaCommands(cmds);
        }
    }
}

