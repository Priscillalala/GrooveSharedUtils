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
                    contentPack.projectilePrefabs.AddHash(g);
                }
                else if (g.GetComponent<CharacterMaster>())
                {
                    contentPack.masterPrefabs.AddHash(g);
                }
                else if (g.GetComponent<CharacterBody>())
                {
                    contentPack.bodyPrefabs.AddHash(g);
                }
                else if (g.GetComponent<Run>())
                {
                    contentPack.gameModePrefabs.AddHash(g);
                }
                else if (g.GetComponent<NetworkIdentity>())
                {
                    contentPack.networkedObjectPrefabs.AddHash(g);
                }
            };
            map[typeof(ModdedScriptableObject)] = (object obj) =>
            {
                ((ModdedScriptableObject)obj).Register();
            };
            if (GSUtil.ModLoadedCached("com.bepis.r2api.items"))
            {
                AddItemAPIToMap(map);
            }
            if (GSUtil.ModLoadedCached("com.bepis.r2api.colors"))
            {
                AddColorAPIToMap(map);
            }
            map[typeof(Type)] = (object obj) =>
            {
                Type t = (Type)obj;
                if (t.IsSubclassOf(typeof(EntityStates.EntityState)))
                {
                    contentPack.entityStateTypes.AddHash(t);
                }
            };
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
        internal void ResolveMap()
        {
            map.ResolveAllAssetCollections();
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


    }
}
