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
    public class DamageTypeFrame : BaseFrame<DamageTypeFrame>
    {
        public string name;
        public ModdedDamageTypeDef ModdedDamageTypeDef { get; private set;}
        internal override object[] Assets => new object[] { ModdedDamageTypeDef };
        internal override void BuildInternal()
        {
            ModdedDamageTypeDef = ScriptableObject.CreateInstance<ModdedDamageTypeDef>();
            ModdedDamageTypeDef.cachedName = name;
        }
    }
}
