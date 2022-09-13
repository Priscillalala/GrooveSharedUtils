﻿using BepInEx;
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

namespace GrooveSharedUtils.Frames
{
    public class ItemFrame : BaseFrame<ItemFrame>
    {
        public static ItemRelationshipType contagiousItemRelationshipType = GSUtil.LegacyLoad<ItemRelationshipType>("ItemRelationships/ContagiousItem");

        public ItemDef ItemDef { get; private set; }
        public ItemRelationshipProvider[] ItemRelationshipProviders { get; private set; }

        public string name;
        public string overrideNameToken = null;
        public string overridePickupToken = null;
        public string overrideDescriptionToken = null;
        public string overrideLoreToken = null;
        public Sprite icon = null;
        public GameObject pickupModelPrefab = null;
        public ItemTier itemTier = ItemTier.NoTier;
        public ItemTierDef overrideItemTierDef = null;
        public bool canRemove = true;
        public bool hidden = false;
        public ItemTag[] itemTags = Array.Empty<ItemTag>();
        public UnlockableDef unlockableDef = null;
        public ItemDef[] itemsToCorrupt = Array.Empty<ItemDef>();

        internal override object[] Assets => new object[] { ItemDef, ItemRelationshipProviders };

        internal override void BuildInternal()
        {
            //Util.Log("name: " + name);
            //string safeName = name.FormatCharacters((char c) => { return true; /*!c.IsSpecialCharacter() && !char.IsWhiteSpace(c);*/ });
            string token = name.ToUpperInvariant();
            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = name;
            ItemDef.nameToken = overrideNameToken ?? string.Format("ITEM_{0}_NAME", token);
            ItemDef.pickupToken = overridePickupToken ?? string.Format("ITEM_{0}_PICKUP", token);
            ItemDef.descriptionToken = overrideDescriptionToken ?? string.Format("ITEM_{0}_DESC", token);
            ItemDef.loreToken = overrideLoreToken ?? string.Format("ITEM_{0}_LORE", token);
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
