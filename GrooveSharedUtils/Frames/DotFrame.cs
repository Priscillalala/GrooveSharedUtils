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
    public class DotFrame : DotFrame<DotFrame, ModdedDotDef> { }
    public class DotFrame<TModdedDotDef> : DotFrame<DotFrame<TModdedDotDef>, TModdedDotDef>
        where TModdedDotDef : ModdedDotDef { }
    public abstract class DotFrame<TFrame, TModdedDotDef> : Frame<TFrame> 
        where TFrame : DotFrame<TFrame, TModdedDotDef>
        where TModdedDotDef : ModdedDotDef
    {
        public string name;
        public BuffDef associatedBuff = null;
        public DotAPI.CustomDotBehaviour customDotBehaviour = null;
        public DotAPI.CustomDotVisual customDotVisual = null;
        public float damageCoefficientPerSecond = 1f;
        public DamageColorIndex damageColorIndex = DamageColorIndex.Default;        
        public float interval = 1f;
        public bool resetTimerOnAdd = false;
        public BuffDef terminalTimedBuff = null;
        public float terminalTimedBuffDuration;
        public TModdedDotDef ModdedDotDef { get; private set; }
        protected override IEnumerable GetAssets()
        {
            yield return ModdedDotDef;
        }
        protected internal override void BuildInternal([CanBeNull] ModPlugin callingMod)
        {
            ModdedDotDef = ScriptableObject.CreateInstance<TModdedDotDef>();
            ModdedDotDef.cachedName = name;
            ModdedDotDef.associatedBuff = associatedBuff;
            if(associatedBuff && associatedBuff.isDebuff)
            {
                GSUtil.Log(BepInEx.Logging.LogLevel.Warning, string.Format("ModdedDotDef {0} has associated buff {1} marked as isDebuff, this will count as 2 debuffs!", name, associatedBuff.name));
            }
            ModdedDotDef.customDotBehaviour = customDotBehaviour;
            ModdedDotDef.customDotVisual = customDotVisual;
            ModdedDotDef.damageCoefficient = damageCoefficientPerSecond * interval;
            ModdedDotDef.damageColorIndex = damageColorIndex;
            ModdedDotDef.interval = interval;
            ModdedDotDef.moddedDamageColor = null;
            ModdedDotDef.resetTimerOnAdd = false;
            ModdedDotDef.terminalTimedBuff = terminalTimedBuff;
            ModdedDotDef.terminalTimedBuffDuration = terminalTimedBuffDuration;
        }
    }
}
