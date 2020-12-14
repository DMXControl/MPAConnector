using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPAConnector.json;

namespace MPAConnector.Elements
{
    public class MPAMotorfader : AbstractMPAElement, IMPAMotorfader
    {
        public event EventHandler<FaderChangedEventArgs> ValueChanged;
        public event EventHandler<ButtonChangedEventArgs> TouchedChanged;

        private ushort _value;

        public MPAMotorfader(int index, MPATile parent) : base(index, parent)
        {
        }

        public bool Touched { get; private set; }

        public ushort Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    ValueChanged?.Invoke(this, new FaderChangedEventArgs(value));

                    var e = new Event()
                    {
                        MsgType = "event",
                        Nid = Parent.ShortID,
                        Com = "MOTORFADER",
                        Idx = Index,
                        Cmd = "UPDATE",
                        Val = value
                    };
                    Parent.Parent.Connector.SendEvent(e);
                }
            }
        }

        protected override bool ProcessEventInternal(string cmd, string com, int idx, int val)
        {
            if (!"MOTORFADER".Equals(com, StringComparison.InvariantCultureIgnoreCase))
                return false;

            bool ret = true;
            switch (cmd?.ToUpperInvariant())
            {
                default: ret = false;
                    break;

                case "UPDATED":
                    if (_value != (ushort) val)
                    {
                        _value = (ushort) val;
                        ValueChanged?.Invoke(this, new FaderChangedEventArgs(_value));
                    }
                    break;

                case "TOUCHED":
                    if (!Touched)
                    {
                        Touched = true;
                        TouchedChanged?.Invoke(this, new ButtonChangedEventArgs(Touched));
                    }
                    break;

                case "RELEASED":
                    if (Touched)
                    {
                        Touched = false;
                        TouchedChanged?.Invoke(this, new ButtonChangedEventArgs(Touched));
                    }
                    break;
            }

            return ret;
        }
    }
}
