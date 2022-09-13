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
using GrooveSharedUtils.Attributes;
using GrooveSharedUtils.MSUMaterialControllerComponents;

namespace GrooveSharedUtils
{
    [AssetDisplayCase]
    [IgnoreModule]
    public class ModModuleExample : BaseModModule<ModModuleExample>
    {
        public static ItemDef SoulTaker;
        public static AssetBundle TestAssetBundle;
        public override void OnModInit()
        {
        }
        public override void OnCollectContent(AssetStream sasset)
        {
            GameObject pickupPrefab = TestAssetBundle.LoadAsset<GameObject>("SoulTaker.prefab");
            pickupPrefab.AddComponent<HGStandardController>();
            var output = new ItemFrame
            {
                name = "SoulTaker",
                pickupModelPrefab = pickupPrefab,
                itemTier = ItemTier.VoidTier3,
                itemTags = new ItemTag[] { ItemTag.Damage },
                itemsToCorrupt = new ItemDef[] { GSUtil.LegacyLoad<ItemDef>("ItemDefs/Dagger") },
            }.Build();
            output.ItemDef.nameToken.AddLang("Soul Taker");
            output.ItemDef.pickupToken.AddLang("Pickup");
            output.ItemDef.descriptionToken.AddLang("Description");
            sasset.Add(output);
        }
    }
}
