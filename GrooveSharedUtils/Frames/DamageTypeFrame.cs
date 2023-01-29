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
    public class DamageTypeFrame : DamageTypeFrame<DamageTypeFrame, ModdedDamageTypeDef> { }
    public class DamageTypeFrame<TModdedDamageTypeDef> : DamageTypeFrame<DamageTypeFrame<TModdedDamageTypeDef>, TModdedDamageTypeDef> 
        where TModdedDamageTypeDef : ModdedDamageTypeDef { }
    public abstract class DamageTypeFrame<TFrame, TModdedDamageTypeDef> : Frame<TFrame> 
        where TFrame : DamageTypeFrame<TFrame, TModdedDamageTypeDef>, new()
        where TModdedDamageTypeDef : ModdedDamageTypeDef
    {
        public static TFrame Create(string name)
        {
            TFrame frame = new TFrame();
            frame.name = name;
            return frame;
        }

        public string name;
        public TModdedDamageTypeDef ModdedDamageTypeDef { get; private set;}
        protected override IEnumerable GetAssets()
        {
            yield return ModdedDamageTypeDef;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            ModdedDamageTypeDef = ScriptableObject.CreateInstance<TModdedDamageTypeDef>();
            ModdedDamageTypeDef.cachedName = name;
        }
    }
}
