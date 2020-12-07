using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TinkersToolbox.Utils;
using Vintagestory.API.Common;
using Vintagestory.Server;

namespace TinkersToolbox.Patches.Server
{
    [HarmonyPatch(typeof(ServerSystemBlockSimulation), "TryModifyBlockInWorld")]
    class ServerSystemBlockSimulation_TryModifyBlockInWorld
    {
        static FieldInfo f_ToolTier = AccessTools.Field(typeof(CollectibleObject), "ToolTier");
        static MethodInfo m_GetToolTier = SymbolExtensions.GetMethodInfo((IItemStack stack) => ModularItemHelper.GetToolTier(stack));

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; ++i)
            {
                CodeInstruction inst = codes[i];
                if (inst.LoadsField(f_ToolTier))
                {
                    codes[i - 1] = new CodeInstruction(OpCodes.Call, m_GetToolTier);
                    codes[i].opcode = OpCodes.Nop;
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }
}
