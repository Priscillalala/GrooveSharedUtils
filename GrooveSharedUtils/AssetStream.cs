using BepInEx;
using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using MonoMod.Cil;
using BepInEx.Bootstrap;
using Mono.Cecil.Cil;
using UnityEngine;
using System.Collections.Generic;
using RoR2;
using R2API.ScriptableObjects;
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using System.Runtime.CompilerServices;
using System.Collections;
using UnityEngine.Events;
using RoR2.ContentManagement;
using RoR2.Projectile;
using UnityEngine.Networking;
using GrooveSharedUtils.ScriptableObjects;
using GrooveSharedUtils.Interfaces;

namespace GrooveSharedUtils
{
    public class AssetStream
    {
        //private IEnumerable<IBaseModule> destinations;
        private BaseModPlugin plugin;
        private AssetToContentMap map;
        //private List<object> foundContent = new List<object>();
        public AssetStream(BaseModPlugin plugin)
        {
            this.plugin = plugin;            
        }
        internal void GenerateMap()
        {
            ContentPack contentPack = plugin.contentPack;
            map = new AssetToContentMap(contentPack);
            map[typeof(GameObject)] = (object obj) =>
            {
                GameObject g = (GameObject)obj;
                if (g.GetComponent<ProjectileController>())
                {
                    AddHash(contentPack.projectilePrefabs, g);
                }
                else if (g.GetComponent<CharacterMaster>())
                {
                    AddHash(contentPack.masterPrefabs, g);
                }
                else if (g.GetComponent<CharacterBody>())
                {
                    AddHash(contentPack.bodyPrefabs, g);
                }
                else if (g.GetComponent<Run>())
                {
                    AddHash(contentPack.gameModePrefabs, g);
                }
                else if (g.GetComponent<NetworkIdentity>())
                {
                    AddHash(contentPack.networkedObjectPrefabs, g);
                }
            };
            map[typeof(IRegisterable)] = (object obj) =>
            {
                ((IRegisterable)obj).Register();
            };
            map[typeof(Type)] = (object obj) =>
            {
                Type t = (Type)obj;
                if (t.IsSubclassOf(typeof(EntityStates.EntityState)))
                {
                    AddHash(contentPack.entityStateTypes, t);
                }
            };
            map[typeof(AchievementDef)] = (object obj) =>
            {
                EarlyAchievementManager.Add((AchievementDef)obj);
            };
            if (GSUtil.ModLoadedCached("com.bepis.r2api.items"))
            {
                AddItemAPIToMap(map);
            }
            if (GSUtil.ModLoadedCached("com.bepis.r2api.colors"))
            {
                AddColorAPIToMap(map);
            }
            if (GSUtil.ModLoadedCached("com.bepis.r2api.artifactcode"))
            {
                AddArtifactCodeAPIToMap(map);
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal void AddItemAPIToMap(AssetToContentMap map)
        {
            map[typeof(CustomItem)] = (object obj) =>
            {
                ItemAPI.Add((CustomItem)obj);
            };
            map[typeof(CustomEquipment)] = (object obj) =>
            {
                ItemAPI.Add((CustomEquipment)obj);
            };
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal void AddColorAPIToMap(AssetToContentMap map)
        {
            map[typeof(SerializableDamageColor)] = (object obj) =>
            {
                ColorsAPI.AddSerializableDamageColor((SerializableDamageColor)obj);
            };
            map[typeof(SerializableColorCatalogEntry)] = (object obj) =>
            {
                ColorsAPI.AddSerializableColor((SerializableColorCatalogEntry)obj);
            };
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal void AddArtifactCodeAPIToMap(AssetToContentMap map)
        {
            map[typeof(ArtifactCompoundDef)] = (object obj) =>
            {
                ArtifactCodeAPI.AddCompound((ArtifactCompoundDef)obj);
            };
        }
        internal void ResolveMap()
        {
            foreach ((Type type, NamedAssetCollection namedAssetCollection) in map.allAssetCollections)
            {
                ResolveHashDisgusting(namedAssetCollection, type);
            }
        }
        public void Add(params object[] assets)
        {
            Add(asset: assets);
        }
        public void Add(object asset)
        {
            if (asset is IEnumerable enumerable)
            {
                foreach(object obj in enumerable)
                {
                    Add(obj);
                }
                return;
            }
            plugin.AddDisplayAsset(asset);
            map.TryMapAsset(asset);
            if (GSUtil.ModLoadedCached("com.bepis.r2api.content_management") && CheckForR2APISerializableContentPack(asset))
            {
                return;
            }
            if (asset is GameObject gameObject && gameObject.GetComponent<EffectComponent>())
            {
                Add(new EffectDef(gameObject));
                return;
            }

        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal bool CheckForR2APISerializableContentPack(object asset)
        {
            if (asset is R2APISerializableContentPack serializableContentPack)
            {
                FieldInfo[] fields = serializableContentPack.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo fieldInfo = fields[i];
                    if (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        Add(fieldInfo.GetValue(serializableContentPack));
                    }
                }
                return true;
            }
            return false;
        }

        internal static Dictionary<NamedAssetCollection, HashSet<object>> internalAssetCollectionToHash = new Dictionary<NamedAssetCollection, HashSet<object>>();
        internal struct ResolveHashTypeInfo
        {
            public MethodInfo add;
            public MethodInfo ofType;
            public MethodInfo toArray;
        }
        internal static Dictionary<Type, ResolveHashTypeInfo> cachedResolvedHashInfo = new Dictionary<Type, ResolveHashTypeInfo>();
        public static void AddHash(NamedAssetCollection namedAssetCollection, object asset)
        {
            HashSet<object> hashSet = internalAssetCollectionToHash.GetOrCreateValue(namedAssetCollection);
            hashSet.Add(asset);
        }
        public static void ResolveHashDisgusting(NamedAssetCollection namedAssetCollection, Type asType)
        {
            if (internalAssetCollectionToHash.TryGetValue(namedAssetCollection, out HashSet<object> hashSet))
            {
                if (!cachedResolvedHashInfo.TryGetValue(asType, out ResolveHashTypeInfo info))
                {
                    info = new ResolveHashTypeInfo
                    {
                        ofType = typeof(Enumerable).GetMethod("OfType", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(asType),
                        toArray = typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(asType),
                        add = typeof(NamedAssetCollection<>).MakeGenericType(asType).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public),
                    };
                    cachedResolvedHashInfo.Add(asType, info);
                }
                object genericHashSet = info.ofType.Invoke(null, new[] { hashSet });
                object genericArray = info.toArray.Invoke(null, new[] { genericHashSet });
                info.add.Invoke(namedAssetCollection, new[] { genericArray });
                internalAssetCollectionToHash.Remove(namedAssetCollection);
            }
        }

    }
}
