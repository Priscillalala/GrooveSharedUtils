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
    public class CatalogColorFrame : CatalogColorFrame<CatalogColorFrame, ModdedCatalogColorDef> { }
    public class CatalogColorFrame<TModdedCatalogColorDef> : CatalogColorFrame<CatalogColorFrame<TModdedCatalogColorDef>, TModdedCatalogColorDef> 
        where TModdedCatalogColorDef : ModdedCatalogColorDef { }
    public abstract class CatalogColorFrame<TFrame, TModdedCatalogColorDef> : Frame<TFrame> 
        where TFrame : CatalogColorFrame<TFrame, TModdedCatalogColorDef>, new()
        where TModdedCatalogColorDef : ModdedCatalogColorDef
    {
        public static TFrame Create(string name, Color color)
        {
            TFrame frame = new TFrame();
            frame.name = name;
            frame.color = color;
            return frame;
        }

        public string name;
        public Color color;
        public TModdedCatalogColorDef ModdedCatalogColorDef { get; private set; }
        protected override IEnumerator BuildIterator()
        {
            ModdedCatalogColorDef = ScriptableObject.CreateInstance<TModdedCatalogColorDef>();
            ModdedCatalogColorDef.cachedName = name;
            ModdedCatalogColorDef.color = color;
            yield return ModdedCatalogColorDef;
        }
    }
}
