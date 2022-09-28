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
using GrooveSharedUtils.Frames;
using R2API.Utils;
using R2API;
using BepInEx.Configuration;
using System.Runtime.CompilerServices;
using GrooveSharedUtils.Interfaces;

namespace GrooveSharedUtils
{
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(RecalculateStatsAPI))]
    internal class Plugin : BaseModPlugin<Plugin>
    {
               
        public override string PLUGIN_ModName => "GrooveSharedUtils";
        public override string PLUGIN_AuthorName => "groovesalad";
        public override string PLUGIN_VersionNumber => "1.0.0";
        public override string[] PLUGIN_HardDependencyStrings => GSUtil.Array( Common.Dependencies.R2API );

        public override void BeginModInit()
        {
        }
        public override void BeginCollectContent(AssetStream sasset)
        {
        }
    }
}
