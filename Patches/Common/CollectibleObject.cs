using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TinkersToolbox.Utils;
using Vintagestory.API.Common;

namespace TinkersToolbox.Patches.Common
{
    [HarmonyPatch(typeof(Vintagestory.API.Common.CollectibleObject), "OnBlockBreaking")]
    class CollectibleObject
    {
        static FieldInfo f_ToolTier = AccessTools.Field(typeof(Vintagestory.API.Common.CollectibleObject), "ToolTier");
        static FieldInfo f_MiningSpeed = AccessTools.Field(typeof(Vintagestory.API.Common.CollectibleObject), "MiningSpeed");
        static MethodInfo m_GetToolTier = SymbolExtensions.GetMethodInfo((IItemStack stack) => ModularItemHelper.GetToolTier(stack));
        static MethodInfo m_GetMiningSpeed = SymbolExtensions.GetMethodInfo((ItemSlot slot) => ModularItemHelper.GetMiningSpeedDict(slot));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
                else if (inst.LoadsField(f_MiningSpeed))
                {
                    codes[i - 1] = new CodeInstruction(OpCodes.Ldarg_3);
                    codes[i] = new CodeInstruction(OpCodes.Call, m_GetMiningSpeed);
                }
            }

            return codes.AsEnumerable();
        }
    }
}
