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

namespace GrooveSharedUtils.ScriptableObjects
{
    [CreateAssetMenu(menuName = "GrooveSharedUtils")]
    public class ExtendedSerializableContentPack : R2APISerializableContentPack
    {
        [Space(5)]
        [Header("Extras")]
        public ModdedDamageTypeDef[] moddedDamageTypes = Array.Empty<ModdedDamageTypeDef>();
        public ModdedCatalogColorDef[] moddedColorCatalogEntries = Array.Empty<ModdedCatalogColorDef>();
        public ModdedDamageColorDef[] moddedDamageColors = Array.Empty<ModdedDamageColorDef>();

    }
}
