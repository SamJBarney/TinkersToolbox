using HarmonyLib;
using TinkersToolbox.Types;
using Vintagestory.API.Common;
using Vintagestory.Server;

namespace TinkersToolbox
{
    public class Patcher
    {
        public static void apply()
        {
            var harmony = new Harmony("net.mask_of_loki.tbox");
            harmony.PatchAll();
        }
    }
}