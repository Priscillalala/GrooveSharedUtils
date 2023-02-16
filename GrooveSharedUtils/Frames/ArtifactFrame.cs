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
using System.Runtime.CompilerServices;

namespace GrooveSharedUtils.Frames
{
    public class ArtifactFrame : ArtifactFrame<ArtifactFrame, ArtifactDef> { }
    public class ArtifactFrame<TArtifactDef> : ArtifactFrame<ArtifactFrame<TArtifactDef>, TArtifactDef> 
        where TArtifactDef : ArtifactDef { }
    public abstract class ArtifactFrame<TFrame, TArtifactDef> : Frame<TFrame>
        where TFrame : ArtifactFrame<TFrame, TArtifactDef>
        where TArtifactDef : ArtifactDef
    {
        public string name;
        public string overrideNameToken = null;
        public string overrideDescriptionToken = null;
        public Sprite enabledIcon = null;
        public Sprite disabledIcon = null;
        public GameObject pickupModelPrefab = null;
        public UnlockableDef unlockableDef = null;
        public ExpansionDef requiredExpansion = null;
        public Action<RunArtifactManager> enabledAction = null;
        public Action<RunArtifactManager> disabledAction = null;
        public ArtifactCodeInfo? artifactCode = null;
        public TArtifactDef ArtifactDef { get; private set; }
        public TFrame SetArtifactCode(int topLeft, int topCenter, int topRight, int middleLeft, int middleCenter, int middleRight, int bottomLeft, int bottomCenter, int bottomRight)
        {
            artifactCode = new ArtifactCodeInfo(topLeft, topCenter, topRight, middleLeft, middleCenter, middleRight, bottomLeft, bottomCenter, bottomRight);
            return this as TFrame;
        }
        public TFrame SetArtifactCode(Vector3Int topRow, Vector3Int middleRow, Vector3Int bottomRow)
        {
            artifactCode = new ArtifactCodeInfo(topRow, middleRow, bottomRow);
            return this as TFrame;
        }
        public TFrame SetArtifactCodeTopRow(int left, int center, int right)
        {
            ArtifactCodeInfo newArtifactCode = artifactCode ?? default;
            newArtifactCode.SetTopRow(left, center, right);
            artifactCode = newArtifactCode;
            return this as TFrame;
        }
        public TFrame SetArtifactCodeMiddleRow(int left, int center, int right)
        {
            ArtifactCodeInfo newArtifactCode = artifactCode ?? default;
            newArtifactCode.SetMiddleRow(left, center, right);
            artifactCode = newArtifactCode;
            return this as TFrame;
        }
        public TFrame SetArtifactCodeBottomRow(int left, int center, int right)
        {
            ArtifactCodeInfo newArtifactCode = artifactCode ?? default;
            newArtifactCode.SetBottomRow(left, center, right);
            artifactCode = newArtifactCode;
            return this as TFrame;
        }
        protected override IEnumerator BuildIterator()
        {
            string token = name.ToUpperInvariant();
            ArtifactDef = ScriptableObject.CreateInstance<TArtifactDef>();
            ArtifactDef.cachedName = name;
            ArtifactDef.nameToken = overrideNameToken ?? $"{settings.generatedTokensPrefix}ARTIFACT_{token}_NAME";
            ArtifactDef.descriptionToken = overrideDescriptionToken ?? $"{settings.generatedTokensPrefix}ARTIFACT_{token}_DESCRIPTION";
            ArtifactDef.smallIconSelectedSprite = enabledIcon;
            ArtifactDef.smallIconDeselectedSprite = disabledIcon;
            ArtifactDef.pickupModelPrefab = pickupModelPrefab;
            ArtifactDef.unlockableDef = unlockableDef;
            ArtifactDef.requiredExpansion = requiredExpansion ?? defaultExpansionDef;
            yield return ArtifactDef;
            if (enabledAction != null)
            {
                ArtifactActionCatalog.AddEnabledAction(ArtifactDef, enabledAction);
            }
            if (disabledAction != null)
            {
                ArtifactActionCatalog.AddDisabledAction(ArtifactDef, disabledAction);
            }
            if (artifactCode != null && GSUtil.ModLoaded("com.bepis.r2api.artifactcode"))
            {
                RegisterArtifactCode((ArtifactCodeInfo)artifactCode);
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void RegisterArtifactCode(ArtifactCodeInfo info)
        {
            ArtifactCode code = info.ToArtifactCode();
            ArtifactCodeAPI.AddCode(ArtifactDef, code);
            UnityEngine.Object.Destroy(code);
        }
    }
}
