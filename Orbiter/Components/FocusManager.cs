using Orbiter.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace Orbiter.Services
{
    public interface IFocusElement
    {
        void GotFocus();

        void LostFocus();

        void Tap();

        void Manipulate(Vector3 relGlobal, Vector3 relCamera, Vector3 relCameraDiff);

        MenuItem[] ContextMenu { get; }
    }

    public class FocusManager : Component
    {
        private OnScreenMenu onScreenMenu;

        public IFocusElement CurrentFocus { get; private set; }

        public IFocusElement DefaultFocus { get; set; }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            if (node != this.Scene) throw new InvalidOperationException("FocusManager should be attached to scene");

            this.onScreenMenu = this.Scene.GetComponent<OnScreenMenu>()
                ?? throw new InvalidOperationException("OnScreenMenu not found");
        }

        public void SetFocus(IFocusElement element)
        {
            this.CurrentFocus?.LostFocus();
            this.CurrentFocus = element ?? this.DefaultFocus ?? throw new InvalidOperationException("CurrentFocus is null!");
            this.CurrentFocus.GotFocus();

            this.onScreenMenu.SetContextMenu(this.CurrentFocus.ContextMenu);
        }

        public bool HandleTap()
        {
            if (this.CurrentFocus == null)
                return false;

            this.CurrentFocus.Tap();
            return true;
        }

        public void Manipulate(Vector3 relGlobal, Vector3 relCamera, Vector3 relCameraDiff)
        {
            this.CurrentFocus?.Manipulate(relGlobal, relCamera, relCameraDiff);
        }
    }
}
