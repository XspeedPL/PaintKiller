using Microsoft.Xna.Framework;
using PaintKilling.Net;
using PaintKilling.Mechanics.Display;

namespace PaintKilling.Mechanics.Game
{
    public class State : BaseState
    {
        public event System.Action<Component> Selected;

        public State(string name, string helpText) : base(name)
        {
            AddComponent(new Label() { Focusable = false, Position = new Vector2(25, -15), Scale = 0.8F, Text = helpText, VerticalAlign = Component.VAlign.Bottom });
        }

        public override void Reset()
        {
            base.Reset();
            if (Components.Count > 0)
            {
                Focused = Components.First;
                while (!Focused.Value.Enabled || !Focused.Value.Focusable)
                    Focused = Focused.Next;
            }
        }

        public override void Update(Controls ctrl)
        {
            if (ctrl.IsFirstPress(0)) Selected?.Invoke(Focused.Value);
            else
            {
                int dirY = ctrl.FirstY;
                if (dirY == 1)
                {
                    do
                    {
                        Focused = Focused.Next;
                        if (Focused == null) Focused = Components.First;
                    }
                    while (!Focused.Value.Enabled || !Focused.Value.Focusable);
                }
                else if (dirY == -1)
                {
                    do
                    {
                        Focused = Focused.Prev;
                        if (Focused == null) Focused = Components.Last;
                    }
                    while (!Focused.Value.Enabled || !Focused.Value.Focusable);
                }
                Focused.Value?.Update(ctrl);
            }
        }
    }
}
