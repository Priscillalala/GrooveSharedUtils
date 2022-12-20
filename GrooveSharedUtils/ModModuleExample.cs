/*using BepInEx;
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
using RoR2.Items;
using GrooveSharedUtils.Interfaces;

namespace GrooveSharedUtils
{
    [AssetDisplayCase]
    [IgnoreModule]
    public class ModModuleExample : BaseModModule<ModModuleExample>
    {
        public static ItemDef SoulTaker;
        public static ItemDef Virus;
        public static AssetBundle MoreItemsAssets;
        public override void OnModInit()
        {
        }
        public override void OnCollectContent(AssetStream sasset)
        {
            GameObject soulTakerpickupPrefab = MoreItemsAssets.LoadAsset<GameObject>("SoulTaker.prefab");
            soulTakerpickupPrefab.AddComponent<HGStandardController>();
            var output = new ItemFrame
            {
                name = "SoulTaker",
                pickupModelPrefab = soulTakerpickupPrefab,
                itemTier = ItemTier.VoidTier3,
                itemTags = new ItemTag[] { ItemTag.Damage },
                itemsToCorrupt = new ItemDef[] { GSUtil.LegacyLoad<ItemDef>("ItemDefs/Dagger") },
            }.Build();
            sasset.Add(output);
            SoulTaker.nameToken.AddLang("Soul Taker");
            SoulTaker.pickupToken.AddLang("Pickup");
            SoulTaker.descriptionToken.AddLang("Description");

            GameObject virusPickupPrefab = MoreItemsAssets.LoadAsset<GameObject>("Petri.prefab");
            virusPickupPrefab.transform.Find("glass").gameObject.AddComponent<HGIntersectionController>();
            virusPickupPrefab.transform.Find("goo").gameObject.AddComponent<HGIntersectionController>();
            virusPickupPrefab.transform.Find("virus").gameObject.AddComponent<HGCloudRemapController>();
            var output2 = new ItemFrame
            {
                name = "Virus",
                pickupModelPrefab = virusPickupPrefab,
                itemTier = ItemTier.VoidTier3,
                itemTags = new ItemTag[] { ItemTag.Damage },
            }.Build();
            sasset.Add(output2);
            Virus.nameToken.AddLang("Soul Taker");
            Virus.pickupToken.AddLang("Pickup");
            Virus.descriptionToken.AddLang("Description");
        }
        public class SoulTakerBehaviour : BaseItemBodyBehavior, IOnGetStatCoefficientsReciever
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = true)]
            public static ItemDef GetItemDef()
            {
                return SoulTaker;
            }

            public void OnGetStatCoefficients(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseCurseAdd += 1f * stack;
            }
        }
    }
}*/
