using Newtonsoft.Json;
using System.Collections.Generic;
using Vintagestory.API;
using Vintagestory.API.Common;

namespace TinkersToolbox.Types
{
    [JsonObject(MemberSerialization.OptIn)]
    class TinkerProperties
    {

        [JsonProperty]
        public bool Enabled = false;

        [JsonProperty]
        public int ToolTier;

        [JsonProperty]
        public string PartType;

        [JsonProperty]
        public string ResultItem;

        [JsonProperty]
        public bool Breaks;

        [JsonProperty]
        public float AttackPower = 0.0f;

        [JsonProperty]
        public Dictionary<EnumBlockMaterial, float> MiningSpeed;

        [JsonProperty]
        public EnumItemDamageSource[] DamagedBy;

        [JsonProperty]
        public bool UseBuiltinSlots = false;

        [JsonProperty]
        public SlotDefinition[] AvailableSlots;

        [JsonProperty]
        public string ToolheadSlot;
    }
}
