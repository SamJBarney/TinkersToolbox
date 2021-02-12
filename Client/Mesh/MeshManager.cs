using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TinkersToolbox.Client.Util;
using TinkersToolbox.Types;
using TinkersToolbox.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace TinkersToolbox.Client.Mesh
{
    class MeshManager
    {
        private static Dictionary<string, AccessLifetime<MeshRef>> MeshRefs = new Dictionary<string, AccessLifetime<MeshRef>>();

        internal static void Init(ClientMain game)
        {
            if (MeshRefs.Count > 0)
            {
                ClearOldEntries(game, -1);
            }
        }

        internal static int ClearOldEntries(ClientMain game, long maxAge)
        {
            ClientPlatformAbstract platform = game.GetType().GetField("Platform", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(game) as ClientPlatformAbstract;
            var invalidEntries = MeshRefs.Where(pair => !pair.Value.IsValid(maxAge));
            foreach (var entry in invalidEntries)
            {
                platform.DeleteMesh(entry.Value.Value);
                MeshRefs.Remove(entry.Key);
            }
            return invalidEntries.Count();
        }

        internal static MeshRef GetMesh(ClientMain game, ItemStack stack)
        {
            string meshId = GetMeshId(game, stack);

            System.Console.WriteLine("Getting mesh for '{0}'", meshId);

            return MeshRefs.ContainsKey(meshId) ? MeshRefs[meshId].Value : GenerateMesh(game, meshId, stack);
        }

        private static string GetMeshId(ClientMain game, ItemStack stack)
        {
            string result = stack.TempAttributes.GetString("meshId");

            return result != null ? result : GenMeshId(game, stack);

        }

        private static string GenMeshId(ClientMain game, ItemStack stack)
        {
            string result = BuildMeshId(game, stack);
            stack.TempAttributes.SetString("meshId", result);
            return result;
        }

        private static string BuildMeshId(ClientMain game, ItemStack stack)
        {
            string result = stack.Item.Code.ToString().Replace(':', '.');
            IModularItem item = stack.Item as IModularItem;
            if (item != null)
            {
                var slots = ModularItemHelper.GetSlots(stack, game);

                foreach (var slot in slots)
                {
                    if (slot.Value != null)
                    {
                        result = result + "." + BuildMeshId(game, slot.Value);
                    }
                }
            }
            return result;
        }

        private static MeshRef GenerateMesh(ClientMain game, string meshId, ItemStack stack)
        {
            MeshRef meshRef = null;
            UnloadableShape shape = new UnloadableShape();

            if (stack.Item.Shape?.Base != null)
                shape.Load(game, new AssetLocationAndSource(stack.Item.Shape.Base));
            if (shape.Textures == null)
                shape.Textures = new Dictionary<string, AssetLocation>();
            if (shape.AttachmentPointsByCode == null)
                shape.AttachmentPointsByCode = new Dictionary<string, AttachmentPoint>();
            Item item = new Item();
            item.Textures = new Dictionary<string, CompositeTexture>();

            foreach (ItemstackAttribute attr in stack.Attributes.GetOrAddTreeAttribute("toolparts").Values)
            {
                ItemStack partstack = attr.GetValue() as ItemStack;
                IToolPart part = partstack.Item as IToolPart;
                if (part != null)
                {
                    if (part.TinkerProps?.ProvidedTextures == null)
                    {
                        partstack.Item.Textures.ToList().ForEach(kp =>
                        {
                            shape.Textures[kp.Key] = kp.Value.Base;
                            item.Textures[kp.Key] = kp.Value;
                        });
                        UnloadableShape tmp = new UnloadableShape();
                        if (!tmp.Load(game, new AssetLocationAndSource(partstack.Item.Shape.Base)))
                            continue;


                        ShapeElement slot = shape.GetElementByName(part.TinkerProps.PartType);

                        if (slot != null)
                        {
                            slot.Children = slot.Children.Concat(tmp.CloneElements()[0].Children).ToArray();
                        }

                        if (tmp.AttachmentPointsByCode != null)
                        {
                            tmp.AttachmentPointsByCode.ToList().ForEach(kp => shape.AttachmentPointsByCode[kp.Key] = kp.Value);
                        }
                    }
                    else
                    {
                        part.TinkerProps.ProvidedTextures.ToList().ForEach(kp =>
                        {
                            shape.Textures[kp.Key] = kp.Value;
                            item.Textures[kp.Key] = new CompositeTexture()
                            {
                                Base = kp.Value.Clone()
                            };
                        });
                    }
                }

            }

            ShapeTesselatorManager manager = game.GetType().GetField("TesselatorManager", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(game) as ShapeTesselatorManager;
            ItemTextureAtlasManager blockAtlas = game.GetType().GetField("ItemAtlasManager", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(game) as ItemTextureAtlasManager;
            ClientPlatformAbstract platform = game.GetType().GetField("Platform", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(game) as ClientPlatformAbstract;
            if (manager != null && blockAtlas != null && platform != null)
            {
                TextureSource source = new TextureSource(game, blockAtlas.Size, item);
                var field = manager.Tesselator.GetType().GetField("meta", BindingFlags.Instance | BindingFlags.NonPublic);
                var oldMeta = field.GetValue(manager.Tesselator) as TesselationMetaData;
                TesselationMetaData meta = oldMeta.Clone();
                meta.texSource = source;
                meta.withJointIds = false;
                MeshData meshData;
                manager.TLTesselator.Value.TesselateShape((Shape)shape, out meshData, new Vec3f(), new Vec3f(), meta);
                meshRef = platform.UploadMesh(meshData);
                MeshRefs[meshId] = new AccessLifetime<MeshRef>(meshRef);
            }
            return meshRef;
        }
    }
}
