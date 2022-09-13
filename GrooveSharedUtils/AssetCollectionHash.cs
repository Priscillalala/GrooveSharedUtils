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
using UnityEngine.Events;

namespace GrooveSharedUtils
{
    public class AssetCollectionHash<TAsset> : HashSet<TAsset>    
    {
        private NamedAssetCollection<TAsset> assetCollection;
        //private AssetStream owner;
        public AssetCollectionHash(AssetToContentMap owner, NamedAssetCollection<TAsset> assetCollection)
        {
            this.assetCollection = assetCollection;
            //this.owner = owner;
            //owner.resolveAssetCollections += OnResolveAssetCollections;
        }
        private void OnResolveAssetCollections()
        {
            assetCollection.Add(this.ToArray());
            this.Clear();
        }

    }
}
