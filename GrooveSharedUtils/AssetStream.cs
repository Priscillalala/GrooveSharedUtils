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
            map[typeof(CustomItem)] = (object obj) =>
            {
                ItemAPI.Add((CustomItem)obj);
            };
            map[typeof(CustomEquipment)] = (object obj) =>
            {
                ItemAPI.Add((CustomEquipment)obj);
            };
            map[typeof(Type)] = (object obj) =>
            {
                Type t = (Type)obj;
                if (t.IsSubclassOf(typeof(EntityStates.EntityState)))
                {
                    contentPack.entityStateTypes.AddHash(t);
                }
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
            if (asset is R2APISerializableContentPack serializableContentPack)
            {
                FieldInfo[] fields = serializableContentPack.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                for(int i = 0; i < fields.Length; i++)
                {
                    FieldInfo fieldInfo = fields[i];
                    if (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        Add(fieldInfo.GetValue(serializableContentPack));
                    }
                }
            }
            if (asset is GameObject gameObject && gameObject.GetComponent<EffectComponent>())
            {
                Add(new EffectDef(gameObject));
            }
            plugin.AddDisplayAsset(asset);
            map.TryMapAsset(asset);
        }
        
    }
}
