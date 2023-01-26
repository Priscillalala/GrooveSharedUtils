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
    public class DamageColorFrame : DamageColorFrame<ModdedDamageColorDef> { }
    public class DamageColorFrame<TModdedDamageColorDef> : BaseFrame where TModdedDamageColorDef : ModdedDamageColorDef
    {
        public string name;
        public Color color;
        public TModdedDamageColorDef ModdedDamageColorDef { get; private set;}
        protected override IEnumerable GetAssets()
        {
            yield return ModdedDamageColorDef;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            ModdedDamageColorDef = ScriptableObject.CreateInstance<TModdedDamageColorDef>();
            ModdedDamageColorDef.cachedName = name;
            ModdedDamageColorDef.color = color;
        }
    }
}
