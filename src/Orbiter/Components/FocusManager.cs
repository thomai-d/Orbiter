using System;
using System.Linq;
using Orbiter.Components;
using Urho;

namespace Orbiter.Components
{
    public interface IFocusElement
    {
        void GotFocus();

        void LostFocus();

        void Tap();

        void Manipulate(Vector3 relGlobal, Vector3 relCamera, Vector3 relCameraDiff);

        void UpdateJoystickInfo(JoystickInfo oldState, JoystickInfo newState);

        MenuItem[] ContextMenu { get; }
    }

    public class FocusManager : Component
    {
        private OnScreenMenu onScreenMenu;

        public IFocusElement CurrentFocus { get; private set; }

        public IFocusElement DefaultFocus { get; set; }

        private JoystickInfo lastJoystickInfo = new JoystickInfo();

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            if (node != this.Scene) throw new InvalidOperationException("FocusManager should be attached to scene");

            this.onScreenMenu = this.Scene.GetComponent<OnScreenMenu>()
                ?? throw new InvalidOperationException("OnScreenMenu not found");
        }

        public void SetFocus(IFocusElement element)
        {
            this.CurrentFocus?.UpdateJoystickInfo(this.lastJoystickInfo, new JoystickInfo());
            this.CurrentFocus?.LostFocus();
            this.CurrentFocus = element ?? this.DefaultFocus ?? throw new InvalidOperationException("CurrentFocus is null!");
            this.CurrentFocus.GotFocus();

            this.onScreenMenu.SetContextMenu(this.CurrentFocus.ContextMenu);
        }

        public void ReleaseFocus(IFocusElement element)
        {
            if (this.IsFocused(element))
                this.SetFocus(null);
        }

        public bool IsFocused(IFocusElement element)
        {
            return this.CurrentFocus?.Equals(element) ?? false;
        }

        public bool HandleTap()
        {
            if (this.CurrentFocus == null)
                return false;

            this.CurrentFocus.Tap();
            return true;
        }

        public bool UpdateJoystickInfo(JoystickInfo info)
        {
            if (this.CurrentFocus == null)
                return false;

            this.CurrentFocus.UpdateJoystickInfo(this.lastJoystickInfo, info);
            this.lastJoystickInfo = info;
            return false;
        }

        public void Manipulate(Vector3 relGlobal, Vector3 relCamera, Vector3 relCameraDiff)
        {
            this.CurrentFocus?.Manipulate(relGlobal, relCamera, relCameraDiff);
        }
    }
}
