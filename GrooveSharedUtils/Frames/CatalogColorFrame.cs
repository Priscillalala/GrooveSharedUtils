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
    public class CatalogColorFrame : CatalogColorFrame<ModdedCatalogColorDef> { }
    public class CatalogColorFrame<TModdedCatalogColorDef> : BaseFrame where TModdedCatalogColorDef : ModdedCatalogColorDef
    {
        public string name;
        public Color color;
        public TModdedCatalogColorDef ModdedCatalogColorDef { get; private set;}
        protected override IEnumerable GetAssets()
        {
            yield return ModdedCatalogColorDef;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            ModdedCatalogColorDef = ScriptableObject.CreateInstance<TModdedCatalogColorDef>();
            ModdedCatalogColorDef.cachedName = name;
            ModdedCatalogColorDef.color = color;
        }
    }
}
