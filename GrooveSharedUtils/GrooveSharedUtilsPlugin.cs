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
    public class GrooveSharedUtilsPlugin : BaseModPlugin
    {
        static GrooveSharedUtilsPlugin()
        {
            globalStubbedShaderPairs["StubbedShader/deferred/standard"] = Common.Shaders.standard;
            globalStubbedShaderPairs["StubbedShader/fx/hgcloudremap"] = Common.Shaders.cloudRemap;
            globalStubbedShaderPairs["StubbedShader/fx/hgopaquecloudremap"] = Common.Shaders.opaqueCloudRemap;
            globalStubbedShaderPairs["StubbedShader/fx/cloudintersectionremap"] = Common.Shaders.intersectionCloudRemap;
            globalStubbedShaderPairs["StubbedShader/fx/hgdistortion"] = Common.Shaders.distortion;
            globalStubbedShaderPairs["StubbedShader/fx/solidparallax"] = Common.Shaders.solidParallax;
            globalStubbedShaderPairs["StubbedShader/fx/hgforwardplanet"] = Common.Shaders.forwardPlanet;
            globalStubbedShaderPairs["StubbedShader/fx/distantwater"] = Common.Shaders.distantWater;
            globalStubbedShaderPairs["StubbedShader/fx/hgtriplanarterrainblend"] = Common.Shaders.triplanarTerrain;
            globalStubbedShaderPairs["StubbedShader/fx/hgsnowtopped"] = Common.Shaders.snowTopped;
        }
        public class AssemblyInfo
        {
            public BaseModPlugin plugin;
            public Dictionary<(string, Type), FieldInfo> assetFieldLocator = new Dictionary<(string, Type), FieldInfo>(AssetFieldLocatorComparer.comparer);
            public Dictionary<string, ConfigFile> configFiles = new Dictionary<string, ConfigFile>();
            public HashSet<object> pendingDisplayAssets = new HashSet<object>();
        }
        public static ConditionalWeakTable<Assembly, AssemblyInfo> assemblyInfos = new ConditionalWeakTable<Assembly, AssemblyInfo>();
        public static AssemblyInfo GetAssemblyInfo(Assembly assembly)
        {
            return assemblyInfos.GetOrCreateValue(assembly);
        }
        public static Dictionary<Type, string> globalTypeToCommonPrefix = new Dictionary<Type, string>()
        {
            { typeof(NetworkSoundEventDef), "nse" },
            { typeof(BuffDef), "bd" },
            { typeof(EliteDef), "ed" },
            { typeof(DirectorCardCategorySelection), "dccs" },
            { typeof(DccsPool), "dp" },
            { typeof(ItemDisplayRuleSet), "idrs" },
        };
        public static Dictionary<Type, HashSet<string>> typeToPossiblePrefixesCache = new Dictionary<Type, HashSet<string>>();
        public static HashSet<Type> configDisabledModuleTypes = new HashSet<Type>();
        public static Dictionary<string, Shader> globalStubbedShaderPairs = new Dictionary<string, Shader>();
        [SystemInitializer]
        public static void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            List<IOnGetStatCoefficientsReciever> getStatCoefficientsRecievers = GetComponentsCache<IOnGetStatCoefficientsReciever>.GetGameObjectComponents(sender.gameObject);
            foreach (IOnGetStatCoefficientsReciever reciever in getStatCoefficientsRecievers)
            {
                reciever.OnGetStatCoefficients(args);
            }
            GetComponentsCache<IOnGetStatCoefficientsReciever>.ReturnBuffer(getStatCoefficientsRecievers);
        }

        public override string PLUGIN_ModName => "GrooveSharedUtils";
        public override string PLUGIN_AuthorName => "groovesalad";
        public override string PLUGIN_VersionNumber => "1.0.0";
        public override string[] PLUGIN_HardDependencyStrings => GSUtil.Array( Common.Dependencies.R2API );

        public override void BeginModInit()
        {
            Logger.LogWarning("Utils Init!");
        }
        public override void BeginCollectContent(AssetStream sasset)
        {
            
        }
    }
}
