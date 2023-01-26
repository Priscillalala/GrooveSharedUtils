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
    public class DamageTypeFrame : DamageTypeFrame<ModdedDamageTypeDef> { }
    public class DamageTypeFrame<TModdedDamageTypeDef> : BaseFrame where TModdedDamageTypeDef : ModdedDamageTypeDef
    {
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
