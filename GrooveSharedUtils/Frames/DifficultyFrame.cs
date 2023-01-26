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
    public class DifficultyFrame : DifficultyFrame<ModdedDifficultyDef> { }
    public class DifficultyFrame<IModdedDifficultyDef> : BaseFrame where IModdedDifficultyDef : ModdedDifficultyDef
    {
        public string name;
        public float scalingValue = 2f;
        public string overrideNameToken = null;
        public string overrideDescriptionToken = null;
        public Sprite icon = null;
        public Color color = Color.white;
        public string serverTag;
        public bool countsAsHardMode = false;
        public IModdedDifficultyDef ModdedDifficultyDef { get; private set; }
        protected override IEnumerable GetAssets()
        {
            yield return ModdedDifficultyDef;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            string token = name.ToUpperInvariant();
            string tokenPrefix = callingMod ? callingMod.adjustedGeneratedTokensPrefix : string.Empty;
            ModdedDifficultyDef = ScriptableObject.CreateInstance<IModdedDifficultyDef>();
            ModdedDifficultyDef.cachedName = name;
            ModdedDifficultyDef.scalingValue = scalingValue;
            ModdedDifficultyDef.nameToken = overrideNameToken ?? $"{tokenPrefix}DIFFICULTY_{token}_NAME";
            ModdedDifficultyDef.descriptionToken = overrideDescriptionToken ?? $"{tokenPrefix}DIFFICULTY_{token}_DESCRIPTION";
            ModdedDifficultyDef.icon = icon;
            ModdedDifficultyDef.color = color;
            ModdedDifficultyDef.serverTag = serverTag;
            ModdedDifficultyDef.countsAsHardMode = countsAsHardMode;
        }
    }
}
