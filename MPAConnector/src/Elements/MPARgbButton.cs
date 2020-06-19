using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPAConnector.json;

namespace MPAConnector.Elements
{
    public class MPARgbButton : AbstractMPAElement, IMPARgbButton
    {
        public event EventHandler<ButtonChangedEventArgs> PressedChanged;

        private Color _color;

        public MPARgbButton(int index, MPATile parent) : base(index, parent)
        {
        }
        
        public bool Pressed { get; private set; }

        public Color ButtonColor
        {
            get { return _color; }
            set
            {
                _color = value;
                var e = new Event()
                {
                    MsgType = "event",
                    Nid = Parent.ShortID,
                    Com = "RGB_LED",
                    Idx = Index,
                    Cmd = "COLOUR",
                    Val = (value.B << 16) | (value.G << 8) | value.R
                };
                Parent.Parent.Connector.SendEvent(e);
            }
        }

        protected override bool ProcessEventInternal(string cmd, string com, int idx, int val)
        {
            if (!"BUTTON".Equals(com, StringComparison.InvariantCultureIgnoreCase))
                return false;

            bool ret = true;
            var old = Pressed;
            switch (cmd?.ToUpperInvariant())
            {
                default: ret = false;
                    break;
                case "PRESSED":
                    Pressed = true;
                    break;
                case "RELEASED":
                    Pressed = false;
                    break;
            }
            if (ret && old != Pressed)
                PressedChanged?.Invoke(this, new ButtonChangedEventArgs(Pressed));
            return ret;
        }
    }
}
