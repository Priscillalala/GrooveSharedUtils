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
using R2API;
using UnityEngine.AddressableAssets;
using HG;
using RoR2.ExpansionManagement;
using JetBrains.Annotations;
using System.Collections;

namespace GrooveSharedUtils.Frames
{
    public class ItemFrame : ItemFrame<ItemDef> { }
    public class ItemFrame<TItemDef> : BaseFrame where TItemDef : ItemDef
    {
        public static ItemRelationshipType contagiousItemRelationshipType = GSUtil.LegacyLoad<ItemRelationshipType>("ItemRelationships/ContagiousItem");

        public TItemDef ItemDef { get; private set; }
        public ItemRelationshipProvider[] ItemRelationshipProviders { get; private set; }

        public string name;
        public string overrideNameToken = null;
        public string overridePickupToken = null;
        public string overrideDescriptionToken = null;
        public string overrideLoreToken = null;
        public Sprite icon = null;
        public GameObject pickupModelPrefab = null;
        public ItemTier itemTier = ItemTier.AssignedAtRuntime;
        public ItemTierDef overrideItemTierDef = null;
        public bool canRemove = true;
        public bool hidden = false;
        public ItemTag[] itemTags = Array.Empty<ItemTag>();
        public UnlockableDef unlockableDef = null;
        public ItemDef[] itemsToCorrupt = Array.Empty<ItemDef>();
        public ExpansionDef requiredExpansion = null;
        protected override IEnumerable GetAssets()
        {
            yield return ItemDef;
            yield return ItemRelationshipProviders;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            string token = name.ToUpperInvariant();
            string tokenPrefix = callingMod ? callingMod.adjustedGeneratedTokensPrefix : string.Empty;
            ItemDef = ScriptableObject.CreateInstance<TItemDef>();
            ItemDef.name = name;
            ItemDef.nameToken = overrideNameToken ?? string.Format("{1}ITEM_{0}_NAME", token, tokenPrefix);
            ItemDef.pickupToken = overridePickupToken ?? string.Format("{1}ITEM_{0}_PICKUP", token, tokenPrefix);
            ItemDef.descriptionToken = overrideDescriptionToken ?? string.Format("{1}ITEM_{0}_DESC", token, tokenPrefix);
            ItemDef.loreToken = overrideLoreToken ?? string.Format("{1}ITEM_{0}_LORE", token, tokenPrefix);
            ItemDef.pickupIconSprite = icon;
            ItemDef.pickupModelPrefab = pickupModelPrefab;
            if (overrideItemTierDef)
            {
                ItemDef._itemTierDef = overrideItemTierDef;
            }
            else
            {
                ItemDef.deprecatedTier = itemTier;
            }
            ItemDef.canRemove = canRemove;
            ItemDef.hidden = hidden;
            ItemDef.tags = itemTags;
            ItemDef.unlockableDef = unlockableDef;
            ItemDef.requiredExpansion = requiredExpansion ?? callingMod?.ENV_DefaultExpansionDef;
            ItemRelationshipProvider[] itemRelationships = Array.Empty<ItemRelationshipProvider>();
            if (itemsToCorrupt.Length > 0)
            {
                ItemRelationshipProvider itemRelationshipProvider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                itemRelationshipProvider.relationshipType = contagiousItemRelationshipType;
                itemRelationshipProvider.relationships = new ItemDef.Pair[itemsToCorrupt.Length];
                for (int i = 0; i < itemsToCorrupt.Length; i++)
                {
                    itemRelationshipProvider.relationships[i] = new ItemDef.Pair
                    {
                        itemDef1 = itemsToCorrupt[i],
                        itemDef2 = ItemDef
                    };
                }
                ArrayUtils.ArrayAppend(ref itemRelationships, itemRelationshipProvider);
            }
            ItemRelationshipProviders = itemRelationships;
        }
    }
}
