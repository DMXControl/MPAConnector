using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using MPAConnector.json;

namespace MPAConnector.Elements
{
    public abstract class AbstractMPAElement : IMPAElement
    {

        protected AbstractMPAElement(int index, MPATile parent)
        {
            this.Index = index;
            this.Parent = parent;
        }

        public int Index { get; }

        public MPATile Parent { get; }

        internal bool ProcessEvent(Event e)
        {
            return ProcessEventInternal(e.Cmd, e.Com, e.Idx, e.Val);
        }

        protected virtual bool ProcessEventInternal(string cmd, string com, int idx, int val)
        {
            return false;
        }
    }
}
