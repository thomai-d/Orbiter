﻿using System;
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
    }

    public class FocusManager : Component
    {
        public IFocusElement CurrentFocus { get; private set; }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            if (node != this.Scene) throw new InvalidOperationException("FocusManager should be attached to scene");
        }

        public void SetFocus(IFocusElement element)
        {
            this.CurrentFocus?.LostFocus();
            this.CurrentFocus = element;
            this.CurrentFocus?.GotFocus();
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