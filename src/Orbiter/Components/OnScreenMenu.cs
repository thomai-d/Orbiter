using Orbiter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Audio;
using Urho.Gui;

namespace Orbiter.Components
{
    // TODO Document, also requirements
    public class OnScreenMenu : Component
    {
        private MovingAverage averageLocation = new MovingAverage(30);
        private Node menuRoot;
        private MenuItem currentOnScreenMenu;
        private SoundSource soundSource;
        private VoiceRecognition voiceRecognition;
        private Node cameraNode;
        private Camera camera;
        private Octree octree;
        private MenuItem[] contextMenu;

        private TaskCompletionSource<bool> showMenuTcs { get; set; }

        public OnScreenMenu()
        {
            this.ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            if (node != this.Scene) throw new InvalidOperationException("OnScreenMenu should be attached to scene");

            this.soundSource = this.Node.CreateComponent<SoundSource>();

            this.voiceRecognition = this.Scene.GetComponent<VoiceRecognition>()
                ?? throw new InvalidOperationException("VoiceRecognition not found");

            this.cameraNode = this.Scene.GetChild("MainCamera", true) 
                ?? throw new InvalidOperationException("'MainCamera' not found");

            this.camera = this.cameraNode.GetComponent<Camera>()
                ?? throw new InvalidOperationException("Camera not found");

            this.octree = this.Scene.GetComponent<Octree>()
                ?? throw new InvalidOperationException("Octree not found");
        }

        public Task ShowMenu()
        {
            var menu = new MenuItem("MENU", string.Empty, this.contextMenu);
            return ShowMenuAsync(menu);
        }

        private async Task ShowMenuAsync(MenuItem menu)
        {
            const float vlineoffset = 0.03f;

            this.currentOnScreenMenu = menu;
            this.menuRoot = this.Node.CreateChild();

            var top = menu.SubItems.Length / 2.0f * vlineoffset + vlineoffset/2;

            var maxWidth = 0f;
            var commands = new Dictionary<string, Action>();
            maxWidth = Math.Max(maxWidth, this.AddItem(top, this.currentOnScreenMenu, isHeadline: true));

            int n = 0;
            foreach (var item in menu.SubItems)
            {
                maxWidth = Math.Max(maxWidth, this.AddItem(top - ++n * vlineoffset, item));

                if (!string.IsNullOrEmpty(item.VoiceCommand)) commands.Add(item.VoiceCommand, () => this.OnItemSelected(item));
            }

            var exitItem = new MenuItem("Exit", () => { });
            this.AddItem(top - ++n * vlineoffset, exitItem);
            commands.Add("Exit", () => this.OnItemSelected(exitItem));

            await this.voiceRecognition.RegisterCommands(commands);

            var backNode = this.menuRoot.CreateChild();
            var back = backNode.CreateComponent<StaticModel>();
            back.ViewMask = 0x80000000; //hide from raycasts
            back.Model = CoreAssets.Models.Box;
            back.SetMaterial(Material.FromColor(new Color(0.2f, 0.2f, 0.2f, 0.8f)));
            backNode.Translate(new Vector3(0, 0, 0.01f));
            backNode.ScaleNode(new Vector3(maxWidth + 0.2f, vlineoffset * (menu.SubItems.Length + 3), 0.01f));

            if (this.showMenuTcs == null)
            {
                this.soundSource.Play(Application.ResourceCache.GetSound("Sounds\\Yay.wav"));
                this.showMenuTcs = new TaskCompletionSource<bool>(null, TaskCreationOptions.RunContinuationsAsynchronously);
            }

            this.averageLocation.Reset();
            this.menuRoot.SetScale(0.0f);
            this.menuRoot.RunActions(new EaseElasticOut(new ScaleTo(0.5f, 1.0f)));

            await this.showMenuTcs.Task;
        }

        public async void SetContextMenu(MenuItem[] items)
        {
            this.contextMenu = items;

            var commands = items
                .Where(c => !string.IsNullOrEmpty(c.VoiceCommand))
                .ToDictionary(i => i.VoiceCommand, i => i.Execute);

            commands.Add("Hey", () => this.ShowMenu());

            await this.voiceRecognition.RegisterCommands(commands);
        }

        protected override void OnUpdate(float timeStep)
        {
            if (this.menuRoot == null)
                return;

            var headPosition = this.cameraNode.WorldPosition;
            var rotation = this.cameraNode.Rotation;

            // Scale / reposition if something is in front of camera.
            var distance = 1.0f;
            var ray = this.camera.GetScreenRay(0.5f, 0.5f);
            var result = this.octree.RaycastSingle(ray, RayQueryLevel.Triangle, 1.0f, DrawableFlags.Geometry, 0x70000000);
            if (result.HasValue && result.Value.Distance < distance)
                distance = result.Value.Distance;
            this.menuRoot.SetScale(distance);

            // Set Position/Rotation
            averageLocation.AddSample(headPosition + (rotation * new Vector3(0, 0, distance)));
            this.menuRoot.SetWorldPosition(averageLocation.Average);
            this.menuRoot.SetWorldRotation(rotation);
        }

        private async void OnItemSelected(MenuItem item)
        {
            this.soundSource.Play(Application.ResourceCache.GetSound("Sounds\\Select.wav"));

            item.Execute();

            this.Node.RemoveChild(this.menuRoot);
            this.menuRoot = null;

            if (item.HasSubItems)
            {
                await this.ShowMenuAsync(item);
                return;
            }

            this.SetContextMenu(this.contextMenu);
            this.showMenuTcs.SetResult(true);
            this.showMenuTcs = null;
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
    }
}

