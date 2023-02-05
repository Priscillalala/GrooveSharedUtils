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
using System.Collections;
using HG;

namespace GrooveSharedUtils.Frames
{
    public class SingleItemDisplayFrame : SingleItemDisplayFrame<SingleItemDisplayFrame> { }
    public abstract class SingleItemDisplayFrame<TFrame> : Frame<TFrame> 
        where TFrame : SingleItemDisplayFrame<TFrame>, new()
    {
        public static TFrame Create(UnityEngine.Object keyAsset, GameObject defaultDisplayPrefab, LimbFlags defaultLimbMask = LimbFlags.None)
        {
            TFrame frame = new TFrame();
            frame.keyAsset = keyAsset;
            frame.defaultDisplayPrefab = defaultDisplayPrefab;
            frame.defaultLimbMask = defaultLimbMask;
            return frame;
        }

        public UnityEngine.Object keyAsset;
        public GameObject defaultDisplayPrefab;
        public LimbFlags defaultLimbMask = LimbFlags.None;
        public (ItemDisplayRuleSet target, ItemDisplayRule rule)[] rules = Array.Empty<(ItemDisplayRuleSet target, ItemDisplayRule rule)>();
        public TFrame Add(ItemDisplayRuleSet idrs, string childName = null, Vector3? localPos = null, Vector3? localAngles = null, Vector3? localScale = null, GameObject overrideDisplayPrefab = null, LimbFlags? overrideLimbMask = null)
        {

            LimbFlags limbMask = overrideLimbMask ?? defaultLimbMask;
            ArrayUtils.ArrayAppend(ref rules, (idrs, new ItemDisplayRule
            {
                childName = childName ?? "Base",
                localPos = localPos ?? Vector3.zero,
                localAngles = localAngles ?? Vector3.zero,
                localScale = localScale ?? Vector3.one,
                followerPrefab = overrideDisplayPrefab ?? defaultDisplayPrefab,
                limbMask = limbMask,
                ruleType = limbMask > LimbFlags.None ? ItemDisplayRuleType.LimbMask : ItemDisplayRuleType.ParentedPrefab,
            }));
            return this as TFrame;
        }
        public TFrame SetRules(params (ItemDisplayRuleSet target, ItemDisplayRule rule)[] rules)
        {
            this.rules = rules;
            return this as TFrame;
        }
        protected override IEnumerable GetAssets()
        {
            yield break;
        }
        protected internal override void BuildForAssembly(Assembly assembly)
        {
            for(int i = 0; i < rules.Length; i++)
            {
                rules[i].target.Add(keyAsset, rules[i].rule);
            }
        }
    }
}
