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
using RoR2.ContentManagement;

namespace GrooveSharedUtils
{
    public class AssetToContentMap : Dictionary<Type, Action<object>>
    {
        public static (FieldInfo, Type)[] contentPackAssetCollectionFields;
        static AssetToContentMap()
        {
            Type namedAssetCollectionType = typeof(NamedAssetCollection);
            //Util.Log("all fields length: " + typeof(ContentPack).GetFields(BindingFlags.Instance | BindingFlags.Public).Length);
            //Util.Log("named asset collection fields length: " + typeof(ContentPack).GetFields(BindingFlags.Instance | BindingFlags.Public).Where((FieldInfo fi) => { return namedAssetCollectionType.IsAssignableFrom(fi.FieldType); }).Count());
            contentPackAssetCollectionFields = typeof(ContentPack).GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where((FieldInfo fi) => { return namedAssetCollectionType.IsAssignableFrom(fi.FieldType); })
                .Select((FieldInfo fi) => { return (fi, fi.FieldType.GetGenericArguments()[0]); }).ToArray();

        }
        public HashSet<(Type, NamedAssetCollection)> allAssetCollections = new HashSet<(Type, NamedAssetCollection)>();
        public Dictionary<Type, Action<object>> assetTypeActionsCache = new Dictionary<Type, Action<object>>();
        public AssetToContentMap(ContentPack contentPack)
        {
            for (int i = 0; i < contentPackAssetCollectionFields.Length; i++)
            {
                (FieldInfo field, Type type) = contentPackAssetCollectionFields[i];
                NamedAssetCollection namedAssetCollection = (NamedAssetCollection)field.GetValue(contentPack);
                allAssetCollections.Add((type, namedAssetCollection));
                if (!ContainsKey(type))
                {
                    Add(type, (object obj) => {
                        GSUtil.Log("add hash: " + obj.GetType());
                        AssetStream.AddHash(namedAssetCollection, obj); 
                    });
                }
            }
            
        }
        public void TryMapAsset(object asset)
        {
            Type assetType = asset.GetType();
            if(assetTypeActionsCache.TryGetValue(assetType, out Action<object> action))
            {
                action.Invoke(asset);
                return;
            }
            List<Action<object>> assetTypeActions = new List<Action<object>>();
            foreach(KeyValuePair<Type, Action<object>> pair in this)
            {
                if (pair.Key.IsAssignableFrom(assetType))
                {
                    //Util.Log("mapping asset, found: " + pair.Key);
                    Action<object> value = pair.Value;
                    value.Invoke(asset);
                    assetTypeActions.Add(value);
                }
            }
            assetTypeActionsCache.Add(assetType, (Action<object>)Delegate.Combine(assetTypeActions.ToArray()));
        }
    }
}
