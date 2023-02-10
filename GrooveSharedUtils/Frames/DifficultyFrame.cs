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
    public class DifficultyFrame : DifficultyFrame<DifficultyFrame, ModdedDifficultyDef> { }
    public class DifficultyFrame<TModdedDifficultyDef> : DifficultyFrame<DifficultyFrame<TModdedDifficultyDef>, TModdedDifficultyDef> 
        where TModdedDifficultyDef : ModdedDifficultyDef { }
    public abstract class DifficultyFrame<TFrame, TModdedDifficultyDef> : Frame<TFrame> 
        where TFrame : DifficultyFrame<TFrame, TModdedDifficultyDef>
        where TModdedDifficultyDef : ModdedDifficultyDef
    {
        public string name;
        public float scalingValue = 2f;
        public string overrideNameToken = null;
        public string overrideDescriptionToken = null;
        public Sprite icon = null;
        public Color color = Color.white;
        public string serverTag;
        public bool countsAsHardMode = false;
        public TModdedDifficultyDef ModdedDifficultyDef { get; private set; }
        protected override IEnumerator BuildIterator()
        {
            string token = name.ToUpperInvariant();
            ModdedDifficultyDef = ScriptableObject.CreateInstance<TModdedDifficultyDef>();
            ModdedDifficultyDef.cachedName = name;
            ModdedDifficultyDef.scalingValue = scalingValue;
            ModdedDifficultyDef.nameToken = overrideNameToken ?? $"{settings.generatedTokensPrefix}DIFFICULTY_{token}_NAME";
            ModdedDifficultyDef.descriptionToken = overrideDescriptionToken ?? $"{settings.generatedTokensPrefix}DIFFICULTY_{token}_DESCRIPTION";
            ModdedDifficultyDef.icon = icon;
            ModdedDifficultyDef.color = color;
            ModdedDifficultyDef.serverTag = serverTag;
            ModdedDifficultyDef.countsAsHardMode = countsAsHardMode;
            yield return ModdedDifficultyDef;
        }
    }
}
