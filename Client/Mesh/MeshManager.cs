using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkersToolbox.Client.Util;
using TinkersToolbox.Types;
using TinkersToolbox.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace TinkersToolbox.Client.Mesh
{
    class MeshManager
    {

        private static bool InitSuccessful = false;

        private static ClientMain Game;

        private static Dictionary<string, AccessLifetime<MeshRef>> MeshRefs;

        internal static void Init(ClientMain game)
        {
            if (!InitSuccessful && game != null)
            {
                InitSuccessful = true;

                Game = game;

                MeshRefs = new Dictionary<string, AccessLifetime<MeshRef>>();
            }
        }

        internal static int ClearOldEntries(long maxAge)
        {
            var invalidEntries = MeshRefs.Where(pair => !pair.Value.IsValid(maxAge));
            foreach (var entry in invalidEntries)
            {
                MeshRefs.Remove(entry.Key);
            }
            return invalidEntries.Count();
        }

        internal static MeshRef GetMesh(ItemStack stack)
        {
            string meshId = GetMeshId(stack);

            return MeshRefs.ContainsKey(meshId) ? MeshRefs[meshId].Value : GenerateMesh(meshId, stack);
        }

        private static string GetMeshId(ItemStack stack)
        {
            string result = stack.TempAttributes.GetString("meshId");

            return result != null ? result : GenMeshId(stack);

        }

        private static string GenMeshId(ItemStack stack)
        {
            string result = stack.Item.Code.ToString();
            IModularItem item = stack.Item as IModularItem;
            if (item != null)
            {
                var slots = ModularItemHelper.GetSlots(stack, Game);

                foreach (var slot in slots)
                {
                    if (slot.Value != null)
                    {
                        result = result + "." + slot.Key;
                    }
                }
            }
            stack.TempAttributes.SetString("meshId", result);
            return result;
        }

        private static MeshRef GenerateMesh(string meshId, ItemStack stack)
        {
            return null;
        }
    }
}
