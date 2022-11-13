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

namespace GrooveSharedUtils.Frames
{
    public class DamageTypeFrame : DamageTypeFrame<ModdedDamageTypeDef> { }
    public class DamageTypeFrame<IModdedDamageTypeDef> : BaseFrame where IModdedDamageTypeDef : ModdedDamageTypeDef
    {
        public string name;
        public IModdedDamageTypeDef ModdedDamageTypeDef { get; private set;}

        protected override IEnumerable<object> Assets => new object[] { ModdedDamageTypeDef };
        protected internal override void BuildInternal(BaseModPlugin callingMod)
        {
            ModdedDamageTypeDef = ScriptableObject.CreateInstance<IModdedDamageTypeDef>();
            ModdedDamageTypeDef.cachedName = name;
        }
    }
}
