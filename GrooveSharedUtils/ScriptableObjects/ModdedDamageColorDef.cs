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
using HG;

namespace GrooveSharedUtils.ScriptableObjects
{
    [CreateAssetMenu(menuName = "GrooveSharedUtils/ModdedDamageColorDef")]
    public class ModdedDamageColorDef : ModdedScriptableObject
    {
        public Color color;
        public DamageColorIndex damageColorIndex { get; set; }
        protected override void RegisterInternal()
        {
			ArrayUtils.ArrayAppend(ref DamageColor.colors, color);
			DamageColorIndex index = (DamageColorIndex)DamageColor.colors.Length - 1;
			damageColorIndex = index;
			moddedDamageColorIndices.Add(damageColorIndex);
		}
        [SystemInitializer]
        public static void Init()
        {
            if (moddedDamageColorIndices.Count > 0)
            {
                On.RoR2.DamageColor.FindColor += DamageColor_FindColor;
            }
        }
        public static HashSet<DamageColorIndex> moddedDamageColorIndices = new HashSet<DamageColorIndex>();
        private static Color DamageColor_FindColor(On.RoR2.DamageColor.orig_FindColor orig, DamageColorIndex colorIndex)
        {
            if (moddedDamageColorIndices.Contains(colorIndex))
            {
                return DamageColor.colors[(int)colorIndex];
            }
            return orig(colorIndex);
        }
    }
}
