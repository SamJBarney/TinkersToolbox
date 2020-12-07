using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkersToolbox.Types;
using TinkersToolbox.Utils;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace TinkersToolbox.Items
{
    class ToolPart : ModularItem, IToolPart
    {
        public override float GetMiningSpeed(IItemStack itemstack, Block block, IPlayer forPlayer)
        {
            float traitRate = 1f;

            if (block.BlockMaterial == EnumBlockMaterial.Ore || block.BlockMaterial == EnumBlockMaterial.Stone)
            {
                traitRate = forPlayer.Entity.Stats.GetBlended("miningSpeed");
            }

            if (TinkerProps.MiningSpeed == null || !TinkerProps.MiningSpeed.ContainsKey(block.BlockMaterial)) return traitRate;


            return TinkerProps.MiningSpeed[block.BlockMaterial] * GlobalConstants.ToolMiningSpeedModifier * traitRate;
        }

        public bool DamagePart(IWorldAccessor world, Entity byEntity, IItemStack itemstack, int amount = 1)
        {
            ITreeAttribute ToolSlots = itemstack.Attributes.GetOrAddTreeAttribute("toolparts");
            IEnumerable<KeyValuePair<string, IAttribute>> validParts = ToolSlots.Where(pair =>
            {
                ItemStack part = (ItemStack)pair.Value.GetValue();

                return part != null && part.Attributes.GetInt("durability", part.Collectible.GetDurability(part)) > 0;
            });

            // Damage sub parts, if there are any
            if (validParts.Count() > 0)
            {
                var pair = validParts.ElementAt(new Random().Next(0, validParts.Count()));
                IItemStack stack = pair.Value.GetValue() as IItemStack;
                ToolPart part = stack.Item as ToolPart;

                bool broken = false;

                if (part != null)
                {
                    broken = part.DamagePart(world, byEntity, stack);
                }

                // If the part is broken, then remove it
                if (broken)
                {
                    RemovePart(itemstack, pair.Key);
                }
            }

            // Damamge the tool

            int leftDurability = itemstack.Attributes.GetInt("durability", GetDurability(itemstack));
            leftDurability -= amount;
            itemstack.Attributes.SetInt("durability", leftDurability);

            if (leftDurability <= 0)
            {
                // Bound minimum durability
                leftDurability = 0;

                if (byEntity is EntityPlayer)
                {
                    IPlayer player = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                    world.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), player, player);
                }
                else
                {
                    world.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), byEntity.Pos.X, byEntity.Pos.Y, byEntity.Pos.Z);
                }

            }

            itemstack.Attributes.SetInt("durability", leftDurability);

            // Notify the containing item if this part has broken
            return TinkerProps.Breaks && leftDurability == 0;
        }
    }
}
