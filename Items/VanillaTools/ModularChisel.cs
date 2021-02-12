using System.Collections.Generic;
using TinkersToolbox.Types;
using TinkersToolbox.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace TinkersToolbox.Items.VanillaTools
{
    class ModularChisel : ItemChisel, IModularTool
    {
        public TinkerProperties TinkerProps { get; set; }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            ModularItemHelper.OnLoaded(api, this);
        }

        public override void DamageItem(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, int amount = 1)
        {
            ModularItemHelper.DamageTool(world, byEntity, itemslot, amount);
        }

        public override float GetMiningSpeed(IItemStack itemstack, Block block, IPlayer forPlayer)
        {
            ItemStack partstack = ModularItemHelper.GetToolheadStack(itemstack);
            ToolPart part = null;

            if (partstack != null)
            {
                partstack.ResolveBlockOrItem(api.World);
                part = (ToolPart)partstack.Item;
            }

            if (part != null)
            {
                return part.GetMiningSpeed(itemstack, block, forPlayer);
            }

            return base.GetMiningSpeed(itemstack, block, forPlayer);
        }

        public bool AddPart(IItemStack stack, string slotName, ItemStack partstack)
        {
            return ModularItemHelper.AddPart(stack, slotName, partstack);
        }

        public SlotDefinition[] GetSlotDefinitions()
        {
            return ModularItemHelper.GetSlotDefinitions(this);
        }

        public Dictionary<string, ItemStack> GetSlots(IItemStack stack)
        {
            return ModularItemHelper.GetSlots(stack, api.World);
        }

        public ItemStack GetToolheadStack(IItemStack stack)
        {
            return ModularItemHelper.GetToolheadStack(stack);
        }

        public int GetToolTier(IItemStack stack)
        {
            return ModularItemHelper.GetToolTier(stack);
        }

        public bool HasNeededParts(ItemStack stack)
        {
            return ModularItemHelper.HasNeededParts(stack);
        }

        public void RecalculateAttributes(IItemStack stack)
        {
            ModularItemHelper.RecalculateAttributes(stack, api.World);

            ItemStack headstack = GetToolheadStack(stack);

            int tooltier = 0;

            if (headstack != null)
            {
                headstack.ResolveBlockOrItem(api.World);
                ToolPart part = (ToolPart)headstack.Item;

                if (part != null)
                {
                    tooltier = part.TinkerProps.ToolTier;
                }
            }

            if (tooltier != 0)
            {
                stack.Attributes.SetInt("tooltier", tooltier);
            }
            else
            {
                stack.Attributes.RemoveAttribute("tooltier");
            }
        }

        public ItemStack RemovePart(IItemStack stack, string slot)
        {
            return ModularItemHelper.RemovePart(stack, slot);
        }
    }
}
