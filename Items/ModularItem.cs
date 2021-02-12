using System.Collections.Generic;
using System.Linq;
using TinkersToolbox.Client.Mesh;
using TinkersToolbox.Types;
using TinkersToolbox.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.Client.NoObf;

namespace TinkersToolbox.Items
{
    abstract class ModularItem : Item, IModularItem
    {
        public TinkerProperties TinkerProps { get; set; }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            ModularItemHelper.OnLoaded(api, this);
        }

        public override bool ShouldDisplayItemDamage(IItemStack itemstack)
        {
            int max = GetDurability(itemstack);
            return max != itemstack.Attributes.GetInt("durability", max);
        }

        public override int GetDurability(IItemStack itemstack)
        {
            return ModularItemHelper.GetDurability(itemstack);
        }

        public Dictionary<string, ItemStack> GetSlots(IItemStack stack)
        {
            return ModularItemHelper.GetSlots(stack, api.World);
        }

        public bool HasNeededParts(ItemStack stack)
        {
            return ModularItemHelper.HasNeededParts(stack);
        }

        public SlotDefinition[] GetSlotDefinitions()
        {
            return ModularItemHelper.GetSlotDefinitions(this as IModularItem);
        }

        public bool AddPart(IItemStack parent, string slotName, ItemStack partstack)
        {
            return ModularItemHelper.AddPart(parent, slotName, partstack);
        }

        public ItemStack RemovePart(IItemStack parent, string slot)
        {
            return ModularItemHelper.RemovePart(parent, slot);
        }

        public virtual void RecalculateAttributes(IItemStack stack)
        {
            ModularItemHelper.RecalculateAttributes(stack, api.World);
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
            if (itemstack.Attributes.GetOrAddTreeAttribute("toolparts").Count > 0)
            {
                MeshRef meshRef = MeshManager.GetMesh(api.World as ClientMain, itemstack);
                System.Console.WriteLine("MeshRef exists: {0}", meshRef != null);
                if (meshRef != null)
                {
                    renderinfo.ModelRef = meshRef;
                }
            }
        }
    }
}
