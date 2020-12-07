using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkersToolbox.Items;
using TinkersToolbox.Types;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace TinkersToolbox.GUI
{
    class GuiDialogTinkerTable : GuiDialogBlockEntity
    {

        public GuiDialogTinkerTable(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
            if (IsDuplicate) return;

            capi.World.Player.InventoryManager.OpenInventory(Inventory);

            Inventory.SlotModified += (slot) => SetupDialog();

            SetupDialog();
        }

        public void SetupDialog()
        {
            ItemSlot hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory == Inventory)
            {
                capi.Input.TriggerOnMouseLeaveSlot(hoveredSlot);
            }
            else
            {
                hoveredSlot = null;
            }

            ElementBounds quernBounds = ElementBounds.Fixed(0, 0, 400, 400);


            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(quernBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);

            ClearComposers();
            SingleComposer = capi.Gui
                .CreateCompo("BlockEntityTinkerTable" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, ElementStdBounds.SlotGrid(EnumDialogArea.CenterMiddle, 0, 0, 1, 1), "targetSlot")
            ;

            IModularItem item = Inventory[0].Itemstack?.Item as IModularItem;
            SlotDefinition[] slotdefs = item?.TinkerProps?.AvailableSlots;



            if (item != null && slotdefs != null)
            {
                var slots = item.GetSlots(Inventory[0].Itemstack);
                for (int i = 0; i < slotdefs.Length && i < Inventory.Count - 1; ++i)
                {
                    var slotdef = slotdefs[i];
                    ItemStack stack = slots[slotdef.SlotName];
                    int invslot = i + 1;
                    if (stack == null)
                    {
                        stack = Inventory[invslot].Itemstack;
                    }
                    Inventory[invslot].Itemstack = stack;
                    SingleComposer = SingleComposer.AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { invslot }, ElementStdBounds.SlotGrid(EnumDialogArea.CenterMiddle, slotdef.OffsetX, slotdef.OffsetY, 1, 1), "partSlot" + invslot.ToString());
                }
            } else if (Inventory[0].Itemstack?.Item == null && Inventory.Count >= 4)
            {
                SingleComposer = SingleComposer
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, ElementStdBounds.SlotGrid(EnumDialogArea.CenterMiddle, 0, -90.0, 1, 1), "partSlot1");
                SingleComposer = SingleComposer
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 3 }, ElementStdBounds.SlotGrid(EnumDialogArea.CenterMiddle, 0, 90.0, 1, 1), "partSlot2");
            }

            SingleComposer = SingleComposer.Compose();

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }
        }

        private void SendInvPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }
    }
}
