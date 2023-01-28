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
using RoR2.Skills;
using JetBrains.Annotations;
using System.Collections;

namespace GrooveSharedUtils.Frames
{
    public class SkillFrame : SkillFrame<SkillFrame, SkillDef> { }
    public class SkillFrame<TSkillDef> : SkillFrame<SkillFrame<TSkillDef>, TSkillDef>
        where TSkillDef : SkillDef { }
    public abstract class SkillFrame<TFrame, TSkillDef> : Frame<TFrame> 
        where TFrame : SkillFrame<TFrame, TSkillDef>
        where TSkillDef : SkillDef
    {
        public string name;
        public Sprite icon;
        public string overrideSkillNameToken = null;
        public string overrideSkillDescriptionToken = null;
        public string[] keywordTokens = Array.Empty<string>();
        public Action<TSkillDef> setupSkillDef = null;
        public SkillFamily[] skillFamiliesToAppend = Array.Empty<SkillFamily>();
        public UnlockableDef unlockableDef = null;
        public TSkillDef SkillDef { get; private set; }
        public TFrame SetKeyworkTokens(params string[] keywordTokens)
        {
            this.keywordTokens = keywordTokens;
            return this as TFrame;
        }
        public TFrame SetSkillFamiliesToAppend(params SkillFamily[] skillFamiliesToAppend)
        {
            this.skillFamiliesToAppend = skillFamiliesToAppend;
            return this as TFrame;
        }
        protected override IEnumerable GetAssets()
        {
            yield return SkillDef;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            SkillDef = ScriptableObject.CreateInstance<TSkillDef>();
            SkillDef.skillName = name;
            string token = name.ToUpperInvariant();
            string tokenPrefix = callingMod ? callingMod.adjustedGeneratedTokensPrefix : string.Empty;
            SkillDef.skillNameToken = overrideSkillNameToken ?? $"{tokenPrefix}SKILL_{token}_NAME";
            SkillDef.skillDescriptionToken = overrideSkillDescriptionToken ?? $"{tokenPrefix}SKILL_{token}_DESC";
            SkillDef.icon = icon;
            SkillDef.keywordTokens = keywordTokens;
            if (setupSkillDef != null)
            {
                setupSkillDef.Invoke(SkillDef);
            }
            for (int i = 0; i < skillFamiliesToAppend.Length; i++)
            {
                SkillFamily skillFamily = skillFamiliesToAppend[i];
                if (!skillFamily) continue;
                SkillFamily.Variant variant = new SkillFamily.Variant
                {
                    skillDef = SkillDef,
                    unlockableDef = unlockableDef,
                    viewableNode = new ViewablesCatalog.Node(SkillDef.skillName, false, null),
                };
                HG.ArrayUtils.ArrayAppend(ref skillFamily.variants, variant);
            }
        }
    }
}
