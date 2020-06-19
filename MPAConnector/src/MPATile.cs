using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPAConnector.Elements;
using MPAConnector.json;

namespace MPAConnector
{
    public class MPATile : IEnumerable<IMPAElement>
    {
        private readonly Dictionary<int, IMPAElement> _elements = new Dictionary<int, IMPAElement>();

        public MPATile(string type, string pid, int nid, MPAChain parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            this.Type = type;
            this.TileID = pid;
            this.ShortID = nid;
            this.Parent = parent;

            CreateElementsFromType();
        }

        public string Type { get; }

        public string TileID { get; }

        public int ShortID { get; }

        public MPAChain Parent { get; }

        private void CreateElementsFromType()
        {
            _elements.Clear();
            switch (Type)
            {
                case "UIM_12ENC_12BUT":
                    for (int i = 0; i < 12; i++)
                        _elements.Add(i, new MPAEncoder(i, i + 12, this));
                    break;
                case "UIM_8ENC_8BUT":
                    for (int i = 0; i < 8; i++)
                        _elements.Add(i, new MPAEncoder(i, i + 8, this));
                    break;
                case "UIM_12RGB_12BUT":
                    for (int i = 0; i < 12; i++)
                        _elements.Add(i, new MPARgbButton(i, this));
                    break;
                case "UIM_8RGB_8BUT":
                    for (int i = 0; i < 8; i++)
                        _elements.Add(i, new MPARgbButton(i, this));
                    break;
                case "UIM_4FAD":
                    for (int i = 0; i < 4; i++)
                        _elements.Add(i, new MPAMotorfader(i, this));
                    break;
            }

        }

        internal bool ProcessEvent(Event e)
        {
            int idx = e.Idx;
            switch (Type)
            {
                case "UIM_12ENC_12BUT":
                    if (idx >= 12) idx -= 12;
                    break;
                case "UIM_8ENC_8BUT":
                    if (idx >= 8) idx -= 8;
                    break;
            }

            IMPAElement elem;
            if (!_elements.TryGetValue(idx, out elem)) return false;

            return (elem as AbstractMPAElement)?.ProcessEvent(e) ?? false;
        }

        public IEnumerator<IMPAElement> GetEnumerator()
        {
            return _elements.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
