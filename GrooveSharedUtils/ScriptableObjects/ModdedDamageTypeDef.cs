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
using System.Runtime.CompilerServices;

namespace GrooveSharedUtils.ScriptableObjects
{
    [CreateAssetMenu(menuName = "GrooveSharedUtils/ModdedDamageTypeDef")]
    public class ModdedDamageTypeDef : ModdedScriptableObject
    {
        public DamageAPI.ModdedDamageType damageTypeIndex { get; set; }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        protected override void RegisterInternal()
        {
            damageTypeIndex = DamageAPI.ReserveDamageType();
		}
    }
}
