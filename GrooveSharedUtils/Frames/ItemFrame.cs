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
    public class ItemFrame : ItemFrame<ItemFrame, ItemDef> { }
    public class ItemFrame<TItemDef> : ItemFrame<ItemFrame<TItemDef>, TItemDef> 
        where TItemDef : ItemDef { }
    public abstract class ItemFrame<TFrame, TItemDef> : Frame<TFrame> 
        where TFrame : ItemFrame<TFrame, TItemDef>
        where TItemDef : ItemDef
    {
        public static LazyAddressable<ItemRelationshipType> contagiousItemRelationshipType = "RoR2/DLC1/Common/ContagiousItem.asset";

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
        public ModelPanelParametersInfo? logbookModelParameters = null;
        public UnlockableDef unlockableDef = null;
        public ItemDef[] itemsToCorrupt = Array.Empty<ItemDef>();
        public ExpansionDef requiredExpansion = null;
        public TItemDef ItemDef { get; private set; }
        public ItemRelationshipProvider[] ItemRelationshipProviders { get; private set; }
        public TFrame SetItemTags(params ItemTag[] itemTags)
        {
            this.itemTags = itemTags;
            return this as TFrame;
        }
        public TFrame SetLogbookModelParameters(Vector3 modelRotation, float minDistance, float maxDistance, Transform focusPoint = null, Transform cameraPosition = null)
        {
            logbookModelParameters = new ModelPanelParametersInfo(modelRotation, minDistance, maxDistance, focusPoint, cameraPosition);
            return this as TFrame;
        }
        public TFrame SetLogbookModelParameters(Quaternion modelRotation, float minDistance, float maxDistance, Transform focusPoint = null, Transform cameraPosition = null)
        {
            logbookModelParameters = new ModelPanelParametersInfo(modelRotation, minDistance, maxDistance, focusPoint, cameraPosition);
            return this as TFrame;
        }
        public TFrame SetItemsToCorrupt(params ItemDef[] itemsToCorrupt)
        {
            this.itemsToCorrupt = itemsToCorrupt;
            return this as TFrame;
        }
        protected override IEnumerator BuildIterator()
        {
            string token = name.ToUpperInvariant();
            ItemDef = ScriptableObject.CreateInstance<TItemDef>();
            ItemDef.name = name;
            ItemDef.nameToken = overrideNameToken ?? string.Format("{1}ITEM_{0}_NAME", token, settings.generatedTokensPrefix);
            ItemDef.pickupToken = overridePickupToken ?? string.Format("{1}ITEM_{0}_PICKUP", token, settings.generatedTokensPrefix);
            ItemDef.descriptionToken = overrideDescriptionToken ?? string.Format("{1}ITEM_{0}_DESC", token, settings.generatedTokensPrefix);
            ItemDef.loreToken = overrideLoreToken ?? string.Format("{1}ITEM_{0}_LORE", token, settings.generatedTokensPrefix);
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
            ItemDef.requiredExpansion = requiredExpansion ?? defaultExpansionDef;
            yield return ItemDef;

            if (logbookModelParameters != null && pickupModelPrefab)
            {
                GSUtil.SetupModelPanelParameters(pickupModelPrefab, (ModelPanelParametersInfo)logbookModelParameters);
            }

            ItemRelationshipProvider[] itemRelationships = Array.Empty<ItemRelationshipProvider>();
            if (itemsToCorrupt.Length > 0)
            {
                ItemRelationshipProvider itemRelationshipProvider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                itemRelationshipProvider.relationshipType = contagiousItemRelationshipType.WaitForCompletion();
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
            yield return ItemRelationshipProviders;
        }
    }
}
