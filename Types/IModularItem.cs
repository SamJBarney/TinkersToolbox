using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace TinkersToolbox.Types
{
    interface IModularItem
    {
        TinkerProperties TinkerProps
        {
            get; set;
        }

        bool ShouldDisplayItemDamage(IItemStack itemstack);

        int GetDurability(IItemStack item);

        Dictionary<string, ItemStack> GetSlots(IItemStack stack);

        bool HasNeededParts(ItemStack stack);

        SlotDefinition[] GetSlotDefinitions();

        bool AddPart(IItemStack stack, string slotName, ItemStack partstack);

        ItemStack RemovePart(IItemStack stack, string slot);

        void RecalculateAttributes(IItemStack stack);

        void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo);
    }
}
