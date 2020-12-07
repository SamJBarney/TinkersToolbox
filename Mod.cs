using TinkersToolbox.BlockEntities;
using TinkersToolbox.Blocks;
using TinkersToolbox.Client.Mesh;
using TinkersToolbox.Client.System;
using TinkersToolbox.Items;
using TinkersToolbox.Items.VanillaTools;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;

namespace TinkersToolbox
{
    class ModCore : ModSystem
    {
        public static ICoreAPI api;
        Item modularTool;

        SystemObjectLifetime OBJSys;

        public static Item GetItem(AssetLocation loc)
        {
            return ModCore.api.World.GetItem(loc);
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            ModCore.api = api;

            // Apply Harmony Patches
            Patcher.apply();

            // Generics
            api.RegisterItemClass("ModularTool", typeof(ModularTool));
            api.RegisterItemClass("ToolPart", typeof(ToolPart));

            // Vanilla Replacements
            api.RegisterItemClass("ModularAxe", typeof(ModularAxe));
            api.RegisterItemClass("ModularChisel", typeof(ModularChisel));
            api.RegisterItemClass("ModularCleaver", typeof(ModularCleaver));
            api.RegisterItemClass("ModularKnife", typeof(ModularKnife));

            // Blocks
            api.RegisterBlockClass("TinkerTable", typeof(TinkerTable));

            // Block Entities
            api.RegisterBlockEntityClass("TinkerTable", typeof(BlockEntityTinkerTable));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            MeshManager.Init(api.World as ClientMain);

            OBJSys = new SystemObjectLifetime(api.World as ClientMain);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            api.RegisterCommand("tboxcreate", "Generates an empty modular tool", "", (player, groupId, args) =>
            {
                if (this.modularTool == null)
                {
                    this.modularTool = ModCore.api.World.GetItem(new AssetLocation("tbox:modulartool"));
                }

                ItemStack modularStack = new ItemStack(modularTool);

                player.InventoryManager.TryGiveItemstack(modularStack, true);
            }, Privilege.chat);

            api.RegisterCommand("tboxadd", "Adds a tool part to the held modular item", "", (player, groupId, args) =>
            {
                ItemSlot slot = player.InventoryManager.ActiveHotbarSlot;
                ModularItem tool = slot.Itemstack?.Item as ModularItem;
                if (tool != null)
                {
                    string toolslot = args.PopWord();
                    ToolPart part = GetItem(new AssetLocation(args.PopWord())) as ToolPart;

                    if (part != null)
                    {
                        if (tool.AddPart(slot.Itemstack, toolslot, new ItemStack(part, 1)))
                        {
                            slot.MarkDirty();
                        }
                        else
                        {
                            player.SendMessage(groupId, "Can't add part to modular item", EnumChatType.CommandError);
                        }
                    }
                    else
                    {
                        player.SendMessage(groupId, "Part does not exist or doesn't have the ToolPart class applied", EnumChatType.CommandError);
                    }
                }
                else
                {
                    player.SendMessage(groupId, "You are not holding a modular item", EnumChatType.CommandError);
                }
            }, Privilege.chat);

            api.RegisterCommand("tboxremove", "Removes a tool part from the held modular item", "", (player, groupId, args) =>
            {
                ItemSlot slot = player.InventoryManager.ActiveHotbarSlot;
                ModularItem tool = slot.Itemstack?.Item as ModularItem;
                if (tool != null)
                {
                    string toolslot = args.PopWord();

                    ItemStack part = tool.RemovePart(slot.Itemstack, toolslot);
                    part.ResolveBlockOrItem(ModCore.api.World);

                    if (part != null)
                    {
                        player.InventoryManager.TryGiveItemstack(part, true);
                    }
                    slot.MarkDirty();
                }
                else
                {
                    player.SendMessage(groupId, "You are not holding a modular item", EnumChatType.CommandError);
                }
            }, Privilege.chat);
        }
    }

}
