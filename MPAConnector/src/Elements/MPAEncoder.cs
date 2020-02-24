using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPAConnector.Elements
{
    public class MPAEncoder : AbstractMPAElement, IMPAEncoder
    {
        public event EventHandler PressedChanged;
        public event EventHandler TouchedChanged;
        public event EventHandler TurnedLeft;
        public event EventHandler TurnedRight;

        public MPAEncoder(int index, int touchIndex, MPATile parent) : base(index, parent)
        {
            TouchIndex = touchIndex;
        }

        public int TouchIndex { get; }

        public bool Pressed { get; private set; }

        public bool Touched { get; private set; }

        protected override bool ProcessEventInternal(string cmd, string com, int idx, int val)
        {
            if ("ENCODER".Equals(com, StringComparison.InvariantCultureIgnoreCase))
            {
                bool ret = true;
                switch (cmd?.ToUpperInvariant())
                {
                    default: ret = false;
                        break;
                    case "TURNED_RIGHT":
                        TurnedRight?.Invoke(this, EventArgs.Empty);
                        break;
                    case "TURNED_LEFT":
                        TurnedLeft?.Invoke(this, EventArgs.Empty);
                        break;

                }
                return ret;
            }

            if ("BUTTON".Equals(com, StringComparison.InvariantCultureIgnoreCase))
            {
                var old = idx == TouchIndex ? Touched : Pressed;
                bool ret = true;
                switch (cmd?.ToUpperInvariant())
                {
                    default:
                        ret = false;
                        break;
                    case "PRESSED":
                        if (idx == TouchIndex) Touched = true;
                        else Pressed = true;
                        break;
                    case "RELEASED":
                        if (idx == TouchIndex) Touched = false;
                        else Pressed = false;
                        break;
                }

                if (ret)
                {
                    if (idx == TouchIndex)
                    {
                        if (old != Touched)
                            TouchedChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else if (old != Pressed)
                        PressedChanged?.Invoke(this, EventArgs.Empty);
                }

                return ret;
            }

            return false;
        }
    }
}
