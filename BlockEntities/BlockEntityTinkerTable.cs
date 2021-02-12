using System;
using System.IO;
using System.Linq;
using TinkersToolbox.GUI;
using TinkersToolbox.Inventories;
using TinkersToolbox.Items;
using TinkersToolbox.Types;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace TinkersToolbox.BlockEntities
{
    class BlockEntityTinkerTable : BlockEntityOpenableContainer
    {
        private TinkerTableInventory inventory;

        private ItemStack lastStack;

        private new GuiDialogTinkerTable invDialog;

        public BlockEntityTinkerTable(): base()
        {
            inventory = new TinkerTableInventory(null, null);

            inventory.OnInventoryClosed += OnInvClosed;
            inventory.OnInventoryOpened += OnInvOpened;
            inventory.SlotModified += OnSlotModified;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            inventory.Pos = Pos;
            lastStack = inventory[0].Itemstack;
        }

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "TinkerTableInv"; }
        }

        public string DialogTitle
        {
            get { return Lang.Get("TinkerTable"); }
        }
        protected virtual void OnInvOpened(IPlayer player)
        {
        }

        protected virtual void OnInvClosed(IPlayer player)
        {
            invDialog?.Dispose();
            invDialog = null;
        }

        private void OnSlotModified(int slot)
        {
            if (Api.Side == EnumAppSide.Server)
            {
                bool dirty = false;
                if (slot == 0)
                {
                    if (Inventory[0].Itemstack == null)
                    {
                        DropInvalidParts();
                        inventory.Resize(4);
                        dirty = true;
                    }
                    else if (Inventory[0].Itemstack != null)
                    {
                        // Drop any items that are not ToolPart into the world
                        DropInvalidParts();

                        IModularItem item = Inventory[0].Itemstack?.Item as IModularItem;

                        if (item != null)
                        {
                            // Fill part slots
                            SlotDefinition[] slotdefs = item.TinkerProps.AvailableSlots;

                            if (slotdefs != null)
                            {
                                inventory.Resize(slotdefs.Length + 1);
                                var slots = item.GetSlots(Inventory[0].Itemstack);
                                for (int i = 0; i < slotdefs.Length; ++ i)
                                {
                                    var toolpart = slots[slotdefs[i].SlotName];

                                    Inventory[i + 1].Itemstack = toolpart;
                                }
                            }
                            else
                            {
                                DropParts();
                                inventory.Resize(1);
                            }
                        }
                        else
                        {
                            // Not a valid IModularItem, so no slots
                            DropParts();
                            inventory.Resize(1);
                        }

                        dirty = true;
                    }
                }
                else
                {
                    if (Inventory[0].Itemstack != null)
                    {
                        int partindex = slot - 1;
                        IModularItem item = Inventory[0].Itemstack.Item as IModularItem;
                        SlotDefinition[] slotdefs = item.TinkerProps.AvailableSlots;

                        // Only apply the part if it inside the number of available slots
                        if (partindex < slotdefs?.Length)
                        {

                            SlotDefinition slotdef = slotdefs[partindex];
                            item.RemovePart(Inventory[0].Itemstack, slotdef.SlotName);

                            ToolPart part = Inventory[slot].Itemstack?.Item as ToolPart;

                            if (part != null)
                            {
                                item.AddPart(Inventory[0].Itemstack, slotdef.SlotName, Inventory[slot].Itemstack);
                            }
                        }

                        if (!item.HasNeededParts(Inventory[0].Itemstack))
                        {
                            DropOptionalParts();
                            Inventory[0].Itemstack = null;
                        }
                    }
                    else
                    {
                        ToolPart toolhead = Inventory[1].Itemstack?.Item as ToolPart;
                        ToolPart handle = Inventory[3].Itemstack?.Item as ToolPart;

                        if (toolhead != null && handle != null && toolhead.TinkerProps.ResultItem != null)
                        {
                            IModularTool result = Api.World.GetItem(new AssetLocation(toolhead.TinkerProps.ResultItem)) as IModularTool;

                            if (result != null)
                            {
                                Inventory[0].Itemstack = new ItemStack(result as Item, 1);

                                if (!(result.AddPart(Inventory[0].Itemstack, "toolhead", Inventory[1].Itemstack) && result.AddPart(Inventory[0].Itemstack, "handle", Inventory[3].Itemstack)))
                                {
                                    Inventory[0].Itemstack = null;
                                }
                            }
                        }
                    }

                    
                    dirty = true;
                }

                if (lastStack?.Item == null || lastStack.Item is IModularItem)
                {
                    lastStack = Inventory[0].Itemstack;
                }

                // Resend the inventory if needed
                if (dirty)
                {
                    byte[] data;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryWriter writer = new BinaryWriter(ms);
                        TreeAttribute tree = new TreeAttribute();
                        inventory.ToTreeAttributes(tree);
                        tree.ToBytes(writer);
                        data = ms.ToArray();
                    }

                    foreach (string guid in Inventory.openedByPlayerGUIds)
                    {
                        IServerPlayer player = Api.World.PlayerByUid(guid) as IServerPlayer;

                        // Make sure that only online players recieve the update
                        if (player.ConnectionState != EnumClientState.Offline)
                        {
                            UpdateInventory(player, data);
                        }
                    }
                }
            }
        }

        public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (Api.World is IServerWorldAccessor)
            {
                byte[] data;

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(ms);
                    writer.Write("BlockEntityTinkerTable");
                    writer.Write(DialogTitle);
                    TreeAttribute tree = new TreeAttribute();
                    inventory.ToTreeAttributes(tree);
                    tree.ToBytes(writer);
                    data = ms.ToArray();

                    ((ICoreServerAPI)Api).Network.SendBlockEntityPacket((IServerPlayer)byPlayer, Pos.X, Pos.Y, Pos.Z, (int)Packet.OpenGUI, data);

                    byPlayer.InventoryManager.OpenInventory(inventory);
                }
            }
            return true;
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            switch (packetid)
            {
                case (int)Packet.OpenGUI:
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        if (invDialog != null)
                        {
                            if (invDialog?.IsOpened() == true)
                            {
                                invDialog.TryClose();
                            }
                            invDialog?.Dispose();
                            invDialog = null;
                            return;
                        }

                        BinaryReader reader = new BinaryReader(ms);

                        string dialogClassName = reader.ReadString();
                        string dialogTitle = reader.ReadString();

                        TreeAttribute tree = new TreeAttribute();
                        tree.FromBytes(reader);

                        Inventory.FromTreeAttributes(tree);
                        Inventory.ResolveBlocksOrItems();

                        invDialog = new GuiDialogTinkerTable(dialogTitle, Inventory, Pos, Api as ICoreClientAPI);

                        invDialog.TryOpen();
                    }
                    break;
                case (int)Packet.CloseGUI:
                    IClientWorldAccessor clientWorld = (IClientWorldAccessor)Api.World;
                    clientWorld.Player.InventoryManager.CloseInventory(Inventory);
                    if (invDialog?.IsOpened() == true) invDialog?.TryClose();
                    invDialog?.Dispose();
                    invDialog = null;
                    break;
                case (int)Packet.UpdateInv:
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        BinaryReader reader = new BinaryReader(ms);

                        TreeAttribute tree = new TreeAttribute();
                        tree.FromBytes(reader);

                        Inventory.FromTreeAttributes(tree);
                        Inventory.ResolveBlocksOrItems();
                        invDialog?.SetupDialog();
                    }
                    break;
            }
        }

        private void UpdateInventory(IServerPlayer player, byte[] data)
        {
            ((ICoreServerAPI)Api).Network.SendBlockEntityPacket(player, Pos.X, Pos.Y, Pos.Z, (int)Packet.UpdateInv, data);
        }

        private void DropInvalidParts()
        {
            Vec3d spawnPos = Pos.ToVec3d();
            spawnPos.Y += 1.1;
            SlotDefinition[] slots = (lastStack?.Item as IModularItem)?.GetSlotDefinitions();

            if (lastStack != null && slots != null)
            {

                for (int i = 1; i < Inventory.Count; ++ i)
                {
                    ItemStack stack = Inventory[i].Itemstack;
                    SlotDefinition slot = slots[i - 1];

                    ToolPart part = stack?.Item as ToolPart;

                    if (part == null || !slot.ValidPartTypes.Contains(part.TinkerProps.PartType))
                    {
                        Api.World.SpawnItemEntity(stack, spawnPos);
                        Inventory[i].Itemstack = null;
                    }
                    else
                    {
                        Inventory[i].Itemstack = null;
                    }
                }
            }
            else
            {
                for (int i = 1; i < Inventory.Count; ++i)
                {
                    ItemStack stack = Inventory[i].Itemstack;

                    Api.World.SpawnItemEntity(stack, spawnPos);
                    Inventory[i].Itemstack = null;
                }
            }
        }

        private void DropParts()
        {
            Vec3d spawnPos = Pos.ToVec3d();
            spawnPos.Y += 1.1;

            for (int i = 1; i < Inventory.Count; ++i)
            {
                ItemStack stack = Inventory[i].Itemstack;

                if (stack?.Item != null)
                {
                    Api.World.SpawnItemEntity(stack, spawnPos);
                    Inventory[i].Itemstack = null;
                }
            }
        }

        private void DropOptionalParts()
        {
            Vec3d spawnPos = Pos.ToVec3d();
            spawnPos.Y += 1.1;

            SlotDefinition[] slots = (Inventory[0].Itemstack.Item as IModularItem).GetSlotDefinitions();

            for (int i = 1; i < Inventory.Count; ++i)
            {
                ItemStack stack = Inventory[i].Itemstack;
                SlotDefinition slot = slots[i - 1];

                ToolPart part = stack?.Item as ToolPart;

                if (part == null || !slot.ValidPartTypes.Contains(part.TinkerProps.PartType) || slot.Optional)
                {
                    Api.World.SpawnItemEntity(stack, spawnPos);
                    Inventory[i].Itemstack = null;
                }
            }
        }

        private enum Packet
        {
            OpenGUI = 1000,
            CloseGUI = 1001,
            UpdateInv = 1002,
        }
    }
}
