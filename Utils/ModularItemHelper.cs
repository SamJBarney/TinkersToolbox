using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkersToolbox.Types;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace TinkersToolbox.Utils
{
    class ModularItemHelper
    {
        public static void OnLoaded(ICoreAPI api, Item item)
        {
            if (item.Attributes != null && item.Attributes.KeyExists("tinkerProps"))
            {
                TinkerProperties TinkerProps = item.Attributes["tinkerProps"].AsObject<TinkerProperties>();

                if (TinkerProps.UseBuiltinSlots)
                {
                    var defs = new SlotDefinition[3];
                    defs[0] = new SlotDefinition();
                    defs[0].SlotName = "toolhead";
                    defs[0].ValidPartTypes = new string[1];
                    defs[0].ValidPartTypes[0] = "toolhead";
                    defs[0].OffsetX = -60.0;
                    defs[0].OffsetY = -90.0;

                    defs[1] = new SlotDefinition();
                    defs[1].SlotName = "binding";
                    defs[1].Optional = true;
                    defs[1].ValidPartTypes = new string[1];
                    defs[1].ValidPartTypes[0] = "binding";
                    defs[1].OffsetX = 60.0;
                    defs[1].OffsetY = -90.0;

                    defs[2] = new SlotDefinition();
                    defs[2].SlotName = "handle";
                    defs[2].ValidPartTypes = new string[1];
                    defs[2].ValidPartTypes[0] = "handle";
                    defs[2].OffsetY = 90.0;

                    if (TinkerProps.AvailableSlots != null)
                    {
                        defs = defs.Concat(TinkerProps.AvailableSlots).ToArray();
                    }

                    TinkerProps.AvailableSlots = defs;
                }

                (item as IModularItem).TinkerProps = TinkerProps;
            }
        }

        public static Dictionary<string, ItemStack> GetSlots(IItemStack stack, IWorldAccessor world)
        {
            Dictionary<string, ItemStack> slots = null;
            IModularItem tool = stack.Item as IModularItem;

            if (tool != null)
            {
                ITreeAttribute ToolSlots = stack.Attributes.GetOrAddTreeAttribute("toolparts");

                slots = new Dictionary<string, ItemStack>();

                foreach (SlotDefinition slot in GetSlotDefinitions(tool))
                {
                    ItemStack partstack = ToolSlots.GetItemstack(slot.SlotName);

                    if (partstack != null)
                    {
                        partstack.ResolveBlockOrItem(world);
                    }

                    slots[slot.SlotName] = partstack;
                }
            }

            return slots;
        }

        public static SlotDefinition[] GetSlotDefinitions(IModularItem item)
        {
            return item.TinkerProps.AvailableSlots;
        }

        public static bool AddPart(IItemStack parent, string slotName, ItemStack partstack)
        {
            IModularItem item = parent.Item as IModularItem;

            IToolPart part = partstack.Item as IToolPart;

            if (item != null && part != null)
            {
                SlotDefinition[] slotdefs = GetSlotDefinitions(item);
                SlotDefinition slot = slotdefs?.First(s => s.SlotName == slotName);
                if (slot?.ValidPartTypes?.Any(type => part.TinkerProps.PartType == type) == true)
                {
                    return ApplyPart(parent, slotName, partstack);
                }
            }

            return false;
        }

        public static ItemStack RemovePart(IItemStack parent, string slot)
        {
            IModularItem item = parent.Item as IModularItem;

            if (item != null)
            {
                ITreeAttribute ToolSlots = parent.Attributes.GetOrAddTreeAttribute("toolparts");
                ItemStack partstack = ToolSlots.GetItemstack(slot);
                ToolSlots.RemoveAttribute(slot);

                if (partstack != null)
                {
                    item.RecalculateAttributes(parent);
                }

                return partstack;
            }

            return null;
        }

        public static void RecalculateAttributes(IItemStack parent, IWorldAccessor world)
        {
            int durability = 0;
            int maxdurability = 0;
            ITreeAttribute ToolSlots = parent.Attributes.GetTreeAttribute("toolparts");

            foreach (KeyValuePair<string, IAttribute> pair in ToolSlots)
            {
                ItemStack stack = pair.Value.GetValue() as ItemStack;
                stack.ResolveBlockOrItem(world);
                IModularItem item = (IModularItem)stack.Item;
                durability += stack.Attributes.GetInt("durability", item != null ? item.GetDurability(stack) : 0);
                maxdurability += stack.Attributes.GetInt("maxdurability", item != null ? item.GetDurability(stack) : 0);
            }

            parent.Attributes.SetInt("maxdurability", maxdurability);

            if (durability != maxdurability)
            {
                parent.Attributes.SetInt("durability", durability);
            }
            else
            {
                parent.Attributes.RemoveAttribute("durability");
            }
        }

        private static bool ApplyPart(IItemStack parent, string slot, ItemStack partstack)
        {
            ITreeAttribute ToolSlots = parent.Attributes.GetOrAddTreeAttribute("toolparts");

            if (!ToolSlots.HasAttribute(slot))
            {
                ToolSlots.SetItemstack(slot, partstack);
                (parent.Item as IModularItem).RecalculateAttributes(parent);

                return true;
            }

            return false;
        }

        public static bool HasNeededParts(ItemStack stack)
        {
            ITreeAttribute ToolSlots = stack.Attributes.GetOrAddTreeAttribute("toolparts");

            foreach (SlotDefinition slotdef in GetSlotDefinitions(stack.Item as IModularItem))
            {
                if (slotdef.Optional == false && !ToolSlots.HasAttribute(slotdef.SlotName))
                {
                    return false;
                }
            }

            return true;
        }

        public static int GetToolTier(IItemStack stack)
        {
            IModularTool tool = stack.Item as IModularTool;

            if (tool == null)
            {
                return stack?.Item?.ToolTier ?? 0;
            }
            else
            {
                return stack.Attributes.GetInt("tooltier", 0);
            }
        }

        public static ItemStack GetToolheadStack(IItemStack stack)
        {
            ItemStack partstack = null;
            if (stack.Item is IModularTool)
            {
                string toolheadslot = (stack.Item as IModularTool).TinkerProps.ToolheadSlot ?? "toolhead";
                ITreeAttribute TinkerSlots = stack.Attributes.GetOrAddTreeAttribute("toolparts");

                partstack = TinkerSlots.GetItemstack(toolheadslot);
            }

            return partstack;
        }

        // Builtin overrides

        public static int GetDurability(IItemStack itemstack)
        {
            return itemstack.Attributes.GetInt("maxdurability", itemstack.Item.Durability);
        }

        public static float OnBlockBreaking(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter, ICoreAPI api)
        {
            Block block = player.Entity.World.BlockAccessor.GetBlock(blockSel.Position);

            Vec3f faceVec = blockSel.Face.Normalf;
            Random rnd = player.Entity.World.Rand;

            ItemStack stack = itemslot.Itemstack;

            Dictionary<EnumBlockMaterial, float> speeds = null;
            ItemStack partstack = ModularItemHelper.GetToolheadStack(stack);
            IToolPart part = null;

            if (partstack != null)
            {
                partstack.ResolveBlockOrItem(api.World);
                part = (IToolPart)partstack.Item;
            }

            if (part != null)
            {
                speeds = part.TinkerProps.MiningSpeed;
            }

            bool cantMine = true;
            cantMine &= block.RequiredMiningTier > 0;
            cantMine &= (part != null && (part.TinkerProps.ToolTier < block.RequiredMiningTier || speeds == null || !speeds.ContainsKey(block.BlockMaterial)) || stack.Attributes.GetInt("durability", GetDurability(stack)) < 1);

            double chance = block.BlockMaterial == EnumBlockMaterial.Ore ? 0.72 : 0.12;

            if ((counter % 5 == 0) && (rnd.NextDouble() < chance || cantMine) && (block.BlockMaterial == EnumBlockMaterial.Stone || block.BlockMaterial == EnumBlockMaterial.Ore) && (stack.Item.Tool == EnumTool.Pickaxe || stack.Item.Tool == EnumTool.Hammer))
            {
                double posx = blockSel.Position.X + blockSel.HitPosition.X;
                double posy = blockSel.Position.Y + blockSel.HitPosition.Y;
                double posz = blockSel.Position.Z + blockSel.HitPosition.Z;

                player.Entity.World.SpawnParticles(new SimpleParticleProperties()
                {
                    MinQuantity = 0,
                    AddQuantity = 8,
                    Color = ColorUtil.ToRgba(255, 255, 255, 128),
                    MinPos = new Vec3d(posx + faceVec.X * 0.01f, posy + faceVec.Y * 0.01f, posz + faceVec.Z * 0.01f),
                    AddPos = new Vec3d(0, 0, 0),
                    MinVelocity = new Vec3f(
                        4 * faceVec.X,
                        4 * faceVec.Y,
                        4 * faceVec.Z
                    ),
                    AddVelocity = new Vec3f(
                        8 * ((float)rnd.NextDouble() - 0.5f),
                        8 * ((float)rnd.NextDouble() - 0.5f),
                        8 * ((float)rnd.NextDouble() - 0.5f)
                    ),
                    LifeLength = 0.025f,
                    GravityEffect = 0f,
                    MinSize = 0.03f,
                    MaxSize = 0.4f,
                    ParticleModel = EnumParticleModel.Cube,
                    VertexFlags = 200,
                    SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.15f)
                }, player);
            }


            if (cantMine)
            {
                return remainingResistance;
            }

            return remainingResistance - GetMiningSpeed(itemslot.Itemstack, block, player, api.World) * dt;
        }

        public static float GetMiningSpeed(IItemStack itemstack, Block block, IPlayer forPlayer, IWorldAccessor world)
        {
            ItemStack partstack = ModularItemHelper.GetToolheadStack(itemstack);
            IToolPart part = null;

            if (partstack != null)
            {
                partstack.ResolveBlockOrItem(world);
                part = (IToolPart)partstack.Item;
            }

            if (part != null)
            {
                return part.GetMiningSpeed(itemstack, block, forPlayer);
            }

            return itemstack.Item.GetMiningSpeed(itemstack, block, forPlayer);
        }

        public static Dictionary<EnumBlockMaterial, float> GetMiningSpeedDict(ItemSlot slot)
        {
            ItemStack itemstack = slot.Itemstack;
            ItemStack partstack = ModularItemHelper.GetToolheadStack(itemstack);
            IToolPart part = null;

            if (partstack != null)
            {
                partstack.ResolveBlockOrItem(ModCore.api.World);
                part = (IToolPart)partstack.Item;
            }

            if (part != null)
            {
                return part.TinkerProps.MiningSpeed;
            }

            return itemstack.Item.MiningSpeed;
        }

        public static void DamageTool(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, int amount = 1)
        {
            ITreeAttribute ToolSlots = itemslot.Itemstack.Attributes.GetOrAddTreeAttribute("toolparts");
            IEnumerable<KeyValuePair<string, IAttribute>> validParts = ToolSlots.Where(pair => {
                ItemStack partstack = (ItemStack)pair.Value.GetValue();
                partstack.ResolveBlockOrItem(world);

                return partstack != null && partstack.Attributes.GetInt("durability", partstack.Item.GetDurability(partstack)) > 0;
            });

            ItemStack itemstack = itemslot.Itemstack;

            bool shouldBreak = false;

            // Damage a subpart if they can be damaged
            if (validParts.Count() > 0)
            {
                ItemStack stack = validParts.ElementAt(new Random().Next(0, validParts.Count())).Value.GetValue() as ItemStack;
                stack.ResolveBlockOrItem(world);
                IToolPart part = (IToolPart)stack.Item;

                bool brokenPart = false;

                if (part != null)
                {
                    brokenPart = part.DamagePart(world, byEntity, stack, amount);
                }

                shouldBreak = brokenPart && !HasNeededParts(itemstack);
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

            if (shouldBreak)
            {
                if (byEntity is EntityPlayer)
                {
                    IPlayer player = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                    world.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), player, player);
                }
                else
                {
                    world.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), byEntity.Pos.X, byEntity.Pos.Y, byEntity.Pos.Z);
                }

                itemslot.Itemstack = null;
            }

            itemslot.MarkDirty();
        }
    }
}
