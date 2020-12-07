using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace TinkersToolbox.Inventories
{
    class TinkerTableInventory : InventoryBase, ISlotProvider
    {
        private ItemSlot[] slots;

        public TinkerTableInventory(string inventoryId, ICoreAPI api): base(inventoryId, api)
        {
            slots = GenEmptySlots(4);
            LimitStackSize();
        }

        public TinkerTableInventory(string className, string instanceId, ICoreAPI api): base(className, instanceId, api)
        {
            slots = GenEmptySlots(4);
            LimitStackSize();
        }

        public override ItemSlot this[int slotId]
        {
            get
            {
                if (slotId < 0 || slotId >= Count) return null;
                return slots[slotId];
            }
            set
            {
                if (slotId < 0 || slotId >= Count) throw new ArgumentOutOfRangeException(nameof(slotId));
                if (value == null) throw new ArgumentNullException(nameof(value));
                slots[slotId] = value;
            }
        }

        public ItemSlot[] Slots
        {
            get { return slots; }
        }

        public override int Count
        {
            get { return slots.Length; }
        }

        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            slots = SlotsFromTreeAttributes(tree, slots);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            SlotsToTreeAttributes(Slots, tree);
        }

        public void Resize(int count)
        {
            if (count > 0)
            {
                ItemSlot[] newSlots = GenEmptySlots(count);
                newSlots[0].Itemstack = slots[0].Itemstack;
                slots = newSlots;
                LimitStackSize();
            }
        }

        private void LimitStackSize()
        {
            foreach (ItemSlot slot in slots)
            {
                slot.MaxSlotStackSize = 1;
            }
        }
    }
}
