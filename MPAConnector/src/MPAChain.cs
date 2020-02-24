using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPAConnector.Elements;

namespace MPAConnector
{
    public class MPAChain : IEnumerable<MPATile>
    {
        private readonly Dictionary<int, MPATile> _tiles = new Dictionary<int, MPATile>();

        public MPAChain(string chainId, MPAGlueConnector connector)
        {
            this.ChainID = chainId;
            this.Connector = connector;
        }

        public string ChainID { get; }

        public MPAGlueConnector Connector { get; }

        public void AddTile(MPATile tile)
        {
            _tiles[tile.ShortID] = tile;
        }

        public MPATile this[int shortId]
        {
            get
            {
                MPATile t;
                if (_tiles.TryGetValue(shortId, out t)) return t;
                return null;
            }
        }

        public IEnumerator<MPATile> GetEnumerator()
        {
            return _tiles.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
