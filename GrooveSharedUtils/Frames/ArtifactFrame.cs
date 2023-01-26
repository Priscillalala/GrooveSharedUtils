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
using RoR2.ExpansionManagement;
using System.Collections;
using JetBrains.Annotations;

namespace GrooveSharedUtils.Frames
{
    public class ArtifactFrame : ArtifactFrame<ArtifactDef> { }
    public class ArtifactFrame<TArtifactDef> : BaseFrame where TArtifactDef : ArtifactDef
    {
        public string name;
        public string overrideNameToken = null;
        public string overrideDescriptionToken = null;
        public Sprite selectedIcon = null;
        public Sprite deselectedIcon = null;
        public GameObject pickupModelPrefab = null;
        public UnlockableDef unlockableDef = null;
        public ExpansionDef requiredExpansion = null;
        public TArtifactDef ArtifactDef { get; private set; }
        protected override IEnumerable GetAssets()
        {
            yield return ArtifactDef;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            string token = name.ToUpperInvariant();
            string tokenPrefix = callingMod ? callingMod.adjustedGeneratedTokensPrefix : string.Empty;
            ArtifactDef = ScriptableObject.CreateInstance<TArtifactDef>();
            ArtifactDef.cachedName = name;
            ArtifactDef.nameToken = overrideNameToken ?? $"{tokenPrefix}ARTIFACT_{token}_NAME";
            ArtifactDef.descriptionToken = overrideDescriptionToken ?? $"{tokenPrefix}ARTIFACT_{token}_DESCRIPTION";
            ArtifactDef.smallIconSelectedSprite = selectedIcon;
            ArtifactDef.smallIconDeselectedSprite = deselectedIcon;
            ArtifactDef.pickupModelPrefab = pickupModelPrefab;
            ArtifactDef.unlockableDef = unlockableDef;
            ArtifactDef.requiredExpansion = requiredExpansion ?? callingMod?.ENV_DefaultExpansionDef;
        }
    }
}
