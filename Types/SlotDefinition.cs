using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkersToolbox.Types
{
    [JsonObject(MemberSerialization.OptIn)]
    class SlotDefinition
    {
        [JsonProperty]
        public string SlotName;

        [JsonProperty]
        public string[] ValidPartTypes;

        [JsonProperty]
        public bool Optional = false;

        [JsonProperty]
        public double OffsetX = 0.0;

        [JsonProperty]
        public double OffsetY = 0.0;
    }
}
