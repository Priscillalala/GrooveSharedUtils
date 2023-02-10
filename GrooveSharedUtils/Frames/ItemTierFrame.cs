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
using GrooveSharedUtils.ScriptableObjects;
using JetBrains.Annotations;
using System.Collections;

namespace GrooveSharedUtils.Frames
{
    public class ItemTierFrame : ItemTierFrame<ItemTierFrame, ItemTierDef> { }
    public class ItemTierFrame<TItemTierDef> : ItemTierFrame<ItemTierFrame<TItemTierDef>, TItemTierDef> 
        where TItemTierDef : ItemTierDef { }
    public abstract class ItemTierFrame<TFrame, TItemTierDef> : Frame<TFrame> 
        where TFrame : ItemTierFrame<TFrame, TItemTierDef>
        where TItemTierDef : ItemTierDef
    {
        public string name;
        public Texture bgIconTexture = null;
        public ColorCatalog.ColorIndex colorIndex = ColorCatalog.ColorIndex.None;
        public ColorCatalog.ColorIndex darkColorIndex = ColorCatalog.ColorIndex.None;
        public bool isDroppable = true;
        public bool canScrap = true;
        public bool canRestack = true;
        public ItemTierDef.PickupRules pickupRules = RoR2.ItemTierDef.PickupRules.Default;
        public GameObject highlightPrefab = null;
        public GameObject dropletDisplayPrefab = null;
        public TItemTierDef ItemTierDef { get; private set; }
        protected override IEnumerator BuildIterator()
        {
            ItemTierDef = ScriptableObject.CreateInstance<TItemTierDef>();
            ItemTierDef.bgIconTexture = bgIconTexture;
            ItemTierDef.colorIndex = colorIndex;
            ItemTierDef.darkColorIndex = darkColorIndex;
            ItemTierDef.isDroppable = isDroppable;
            ItemTierDef.canScrap = canScrap;
            ItemTierDef.canRestack = canRestack;
            ItemTierDef.pickupRules = pickupRules;
            ItemTierDef.highlightPrefab = highlightPrefab;
            ItemTierDef.dropletDisplayPrefab = dropletDisplayPrefab;
            ItemTierDef.tier = ItemTier.AssignedAtRuntime;
            yield return ItemTierDef;
        }
    }
}
