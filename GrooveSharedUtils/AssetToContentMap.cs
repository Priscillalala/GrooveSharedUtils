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
        public List<(Type, NamedAssetCollection)> allAssetCollections = new List<(Type, NamedAssetCollection)>();
        public Dictionary<Type, Action<object>> assetTypeActionsCache = new Dictionary<Type, Action<object>>();
        public Dictionary<NamedAssetCollection, List<object>> plannedAdditions = new Dictionary<NamedAssetCollection, List<object>>();
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
                        //GroovyLogger.Log(BepInEx.Logging.LogLevel.Info, "add hash: " + obj.GetType());
                        PlanAdd(namedAssetCollection, obj); 
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
                    Action<object> value = pair.Value;
                    value.Invoke(asset);
                    assetTypeActions.Add(value);
                }
            }
            assetTypeActionsCache.Add(assetType, (Action<object>)Delegate.Combine(assetTypeActions.ToArray()));
        }
        internal struct ResolveHashTypeInfo
        {
            public MethodInfo add;
            public MethodInfo ofType;
            public MethodInfo toArray;
        }
        internal static Dictionary<Type, ResolveHashTypeInfo> cachedResolvedHashInfo = new Dictionary<Type, ResolveHashTypeInfo>();
        public void PlanAdd(NamedAssetCollection namedAssetCollection, object asset)
        {
            plannedAdditions.GetOrCreateValue(namedAssetCollection).Add(asset);
        }
        public void ResolveAllPlans()
        {
            foreach ((Type type, NamedAssetCollection namedAssetCollection) in allAssetCollections)
            {
                if (plannedAdditions.TryGetValue(namedAssetCollection, out List<object> assets)) 
                {
                    ResolveHashTypeInfo info = cachedResolvedHashInfo.GetOrCreateValue(type, () => new ResolveHashTypeInfo
                    {
                        ofType = typeof(Enumerable).GetMethod("OfType", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type),
                        toArray = typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type),
                        add = typeof(NamedAssetCollection<>).MakeGenericType(type).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public),
                    });
                    object genericEnumerable = info.ofType.Invoke(null, new[] { assets });
                    object genericArray = info.toArray.Invoke(null, new[] { genericEnumerable });
                    info.add.Invoke(namedAssetCollection, new[] { genericArray });
                }
            }
        }
    }
}
