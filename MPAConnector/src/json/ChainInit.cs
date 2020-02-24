using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MPAConnector.json
{
    class ChainInit
    {

        [JsonProperty("chain_id")]
        public string ChainID { get; set; }

        [JsonProperty("board_infos")]
        public BoardInfo[] BoardInfos { get; set; }
    }

    class BoardInfo
    {

        public string Bid { get; set; }
        public string Fid { get; set; }
        public string Fvr { get; set; }
        public int Nid { get; set; }
        public string Pid { get; set; }

        [JsonProperty("usb_linked")]
        public bool UsbLinked { get; set; }


    }
}
