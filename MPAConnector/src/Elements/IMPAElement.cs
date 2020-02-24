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
        event EventHandler ValueChanged;
        event EventHandler TouchedChanged;

        bool Touched { get; }

        ushort Value { get; set; }
    }

    public interface IMPAEncoder : IMPAButton
    {
        event EventHandler TouchedChanged;
        event EventHandler TurnedLeft;
        event EventHandler TurnedRight;


        bool Touched { get; }

    }

    public interface IMPAButton : IMPAElement
    {
        event EventHandler PressedChanged;

        bool Pressed { get; }
    }

    public interface IMPARgbButton : IMPAButton
    {
        Color ButtonColor { get; set; }
    }
}
