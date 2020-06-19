using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPAConnector.json;

namespace MPAConnector.Elements
{
    public interface IMPAElement
    {
        int Index { get; }

        MPATile Parent { get; }
    }

    public interface IMPAMotorfader : IMPAElement
    {
        event EventHandler<FaderChangedEventArgs> ValueChanged;
        event EventHandler<ButtonChangedEventArgs> TouchedChanged;

        bool Touched { get; }

        ushort Value { get; set; }
    }

    public interface IMPAEncoder : IMPAButton
    {
        event EventHandler<ButtonChangedEventArgs> TouchedChanged;
        event EventHandler<EventArgs> TurnedLeft;
        event EventHandler<EventArgs> TurnedRight;


        bool Touched { get; }

    }

    public interface IMPAButton : IMPAElement
    {
        event EventHandler<ButtonChangedEventArgs> PressedChanged;

        bool Pressed { get; }
    }

    public interface IMPARgbButton : IMPAButton
    {
        Color ButtonColor { get; set; }
    }

    public class ButtonChangedEventArgs : EventArgs
    {
        public readonly bool Value;

        public ButtonChangedEventArgs(bool value)
        {
            this.Value = value;
        }
    }

    public class FaderChangedEventArgs : EventArgs
    {
        public readonly ushort Value;

        public FaderChangedEventArgs(ushort value)
        {
            this.Value = value;
        }
    }
}
