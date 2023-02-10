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
using RoR2.ExpansionManagement;
using RoR2.EntitlementManagement;

namespace GrooveSharedUtils.Frames
{
    public class ExpansionFrame : ExpansionFrame<ExpansionFrame, ExpansionDef> { }
    public class ExpansionFrame<TExpansionDef> : ExpansionFrame<ExpansionFrame<TExpansionDef>, TExpansionDef> 
        where TExpansionDef : ExpansionDef
    { }
    public abstract class ExpansionFrame<TFrame, TExpansionDef> : Frame<TFrame> 
        where TFrame : ExpansionFrame<TFrame, TExpansionDef>
        where TExpansionDef : ExpansionDef
    {
        public static LazyAddressable<Sprite> texUnlockIcon = "RoR2/Base/Common/MiscIcons/texUnlockIcon.png"; 
        public string name;
        public Sprite icon = null;
        public Sprite overrideDisabledIcon = null;
        public string overrideNameToken = null;
        public string overrideDescriptionToken = null;
        public EntitlementDef requiredEntitlement = null;
        public GameObject runBehaviorPrefab = null;
        public TExpansionDef ExpansionDef { get; private set; }
        protected override IEnumerator BuildIterator()
        {
            string token = name.ToUpperInvariant();
            ExpansionDef = ScriptableObject.CreateInstance<TExpansionDef>();
            ExpansionDef.name = name;
            ExpansionDef.iconSprite = icon;
            ExpansionDef.disabledIconSprite = overrideDisabledIcon ?? texUnlockIcon.WaitForCompletion();
            ExpansionDef.nameToken = overrideNameToken ?? $"{settings.generatedTokensPrefix}EXPANSION_{token}_NAME";
            ExpansionDef.descriptionToken = overrideDescriptionToken ?? $"{settings.generatedTokensPrefix}EXPANSION_{token}_DESCRIPTION";
            ExpansionDef.requiredEntitlement = requiredEntitlement;
            ExpansionDef.runBehaviorPrefab = runBehaviorPrefab;
            yield return ExpansionDef;
        }
    }
}
