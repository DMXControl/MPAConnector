using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MPAConnector.json
{
    class Event
    {
        [JsonProperty("msg_type")]
        public string MsgType { get; set; }

        [JsonProperty("cmd")]
        public string Cmd { get; set; }

        [JsonProperty("com")]
        public string Com { get; set; }

        [JsonProperty("idx")]
        public int Idx { get; set; }

        [JsonProperty("nid")]
        public int Nid { get; set; }

        [JsonProperty("val")]
        public int Val { get; set; }
    }
}
