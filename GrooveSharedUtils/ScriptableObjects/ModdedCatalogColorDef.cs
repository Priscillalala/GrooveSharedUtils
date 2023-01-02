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
    [CreateAssetMenu(menuName = "GrooveSharedUtils/ModdedCatalogColorDef")]
    public class ModdedCatalogColorDef : ModdedScriptableObject
    {
        public Color color;
        public ColorCatalog.ColorIndex colorCatalogIndex { get; set; }
        protected override void RegisterInternal()
        {
			ColorCatalog.ColorIndex index = (ColorCatalog.ColorIndex)ColorCatalog.indexToColor32.Length;
			ArrayUtils.ArrayAppend(ref ColorCatalog.indexToColor32, color);
			ArrayUtils.ArrayAppend(ref ColorCatalog.indexToHexString, Util.RGBToHex(color));
			colorCatalogIndex = index;
			moddedCatalogColorIndices.Add(index);
		}
        internal static void Init()
        {
            if (moddedCatalogColorIndices.Count > 0)
            {
                On.RoR2.ColorCatalog.GetColor += ColorCatalog_GetColor;
                On.RoR2.ColorCatalog.GetColorHexString += ColorCatalog_GetColorHexString;
            }
        }
        public static HashSet<ColorCatalog.ColorIndex> moddedCatalogColorIndices = new HashSet<ColorCatalog.ColorIndex>();
        private static Color32 ColorCatalog_GetColor(On.RoR2.ColorCatalog.orig_GetColor orig, ColorCatalog.ColorIndex colorIndex)
        {
            if (moddedCatalogColorIndices.Contains(colorIndex))
            {
                return ColorCatalog.indexToColor32[(int)colorIndex];
            }
            return orig(colorIndex);
        }
        private static string ColorCatalog_GetColorHexString(On.RoR2.ColorCatalog.orig_GetColorHexString orig, ColorCatalog.ColorIndex colorIndex)
        {
            if (moddedCatalogColorIndices.Contains(colorIndex))
            {
                return ColorCatalog.indexToHexString[(int)colorIndex];
            }
            return orig(colorIndex);
        }
    }
}
