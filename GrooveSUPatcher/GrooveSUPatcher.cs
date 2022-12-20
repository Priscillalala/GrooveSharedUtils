using System;
using BepInEx;
using BepInEx.Logging;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx.Bootstrap;
using System.Reflection;
using BepInEx.Configuration;
using GrooveSharedUtils.Attributes;

namespace GrooveSharedUtils
{   
    public static class GrooveSUPatcher
    {
        public struct PluginConfigInformation
        {
            public BepInPlugin bepInPlugin;
            public string defaultConfigName;
            public bool trimConfigNameSpaces;
            public ConfigStructure configStructure;
        }
        internal static ManualLogSource logger = Logger.CreateLogSource("GrooveSharedUtilsPatcher");
        public static IEnumerable<string> TargetDLLs { get; } = new string[] { };

        internal static Dictionary<string, PluginInfo> controlledBepInExPlugins = new Dictionary<string, PluginInfo>();
        internal static HashSet<Type> assetCollectionScannedTypes = new HashSet<Type>();
        internal static Dictionary<Assembly, PluginConfigInformation> tempConfigPluginInformation = new Dictionary<Assembly, PluginConfigInformation>();

        const string infoBackingField = "<Info>k__BackingField";
        const string loggerBackingField = "<Logger>k__BackingField";
        const string configBackingField = "<Config>k__BackingField";

        public static void Initialize()
        {
            On.BepInEx.Bootstrap.Chainloader.Initialize += Chainloader_Initialize;
            
            logger.LogWarning("Initializing GSU Patcher");
        }

        private static void BaseUnityPlugin_ctor(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = c.DefineLabel();

            //plugin info
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<BaseModPlugin, PluginInfo>>((baseModPlugin) =>
            {
                return baseModPlugin.SetupPluginInfo();
            });
            c.Emit<BaseUnityPlugin>(OpCodes.Stfld, infoBackingField);
            
            //logger
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<BaseModPlugin, ManualLogSource>>((baseModPlugin) =>
            {
                return baseModPlugin.SetupLogger();
            });
            c.Emit<BaseUnityPlugin>(OpCodes.Stfld, loggerBackingField);

            //config
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<BaseModPlugin, ConfigFile>>((baseModPlugin) =>
            {
                return baseModPlugin.SetupConfigFile();
            });
            c.Emit<BaseUnityPlugin>(OpCodes.Stfld, configBackingField);

            c.Emit(OpCodes.Ret);
            Instruction instruction = c.Instrs[c.Index];
            c.Index = 0;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<BaseUnityPlugin, bool>>((baseUnityPlugin) =>
            {
                return baseUnityPlugin is BaseModPlugin;
            });
            c.Emit(OpCodes.Brfalse_S, instruction);
        }

        private static void Chainloader_Initialize(On.BepInEx.Bootstrap.Chainloader.orig_Initialize orig, string gameExePath, bool startConsole, ICollection<LogEventArgs> preloaderLogEvents)
        {
            logger.LogWarning("Chainloader init");
            IL.BepInEx.BaseUnityPlugin.ctor += BaseUnityPlugin_ctor;
            IL.BepInEx.Bootstrap.Chainloader.Start += Chainloader_Start;
            foreach (string path in Directory.EnumerateFiles(Path.GetFullPath(Paths.PluginPath), "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    if (!path.Contains("MMHOOK") && !path.Contains("R2API"))
                    {
                        HandleAssembly(path);
                    }
                }
                catch (Exception ex) { logger.LogError(ex.ToString()); }
            }

            LanguageCollectionManager.Init();
            List<ConfigurableAttribute> configurableAttributes = new List<ConfigurableAttribute>();
            HG.Reflection.SearchableAttribute.GetInstances(configurableAttributes);
            foreach (ConfigurableAttribute attribute in configurableAttributes)
            {
                BindConfigurableAttribute(attribute);
            }
            tempConfigPluginInformation.Clear();
            orig(gameExePath, startConsole, preloaderLogEvents);
        }
        private static void Chainloader_Start(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int pluginDictLocIndex = -1;

            bool ILFound = c.TryGotoNext(MoveType.After,
                x => x.MatchCall(typeof(TypeLoader), nameof(TypeLoader.FindPluginTypes)),
                x => x.MatchStloc(out pluginDictLocIndex)
                );

            if (ILFound)
            {
                c.Emit(OpCodes.Ldloc, pluginDictLocIndex);
                c.EmitDelegate<Func<Dictionary<string, List<PluginInfo>>, Dictionary<string, List<PluginInfo>>>>((pluginDict) =>
                {
                    foreach (KeyValuePair<string, PluginInfo> pair in controlledBepInExPlugins)
                    {
                        List<PluginInfo> previousPluginInfos = pluginDict.GetOrCreateValue(pair.Key);
                        try
                        {
                            previousPluginInfos.Add(pair.Value);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.ToString());
                        }
                    }
                    return pluginDict;
                });
                c.Emit(OpCodes.Stloc, pluginDictLocIndex);
            }
            else { logger.LogError("Chainloader IL hook failed!"); }
        }
        private static void HandleAssembly(string path)
        {
            logger.LogInfo("Handling Assembly: " + Path.GetFileName(path));
            Assembly assembly = Assembly.LoadFile(path);
            
            if(assembly.GetReferencedAssemblies().All((AssemblyName name) => name.Name != "GrooveSharedUtils") && assembly.GetName().Name != "GrooveSharedUtils")
            {
                return;
            }
            //logger.LogInfo("Valid Assembly: " + Path.GetFileName(path));
            Type[] types = assembly.GetTypes();
            int length = types.Length;
            //logger.LogInfo(length);
            bool foundBasePlugin = false;
            //HG.Reflection.SearchableAttribute.;
            for (int i = 0; i < length; i++)
            {
                Type type = types[i];
                if (type.GetCustomAttribute<AssetDisplayCaseAttribute>() != null)
                {
                    FindAssetFields(type, AssemblyInfo.Get(assembly).assetFieldLocator);
                }

                if (!type.IsAbstract && type.IsSubclassOf(typeof(BaseModPlugin)))
                {
                    if (foundBasePlugin)
                    {
                        logger.LogWarning(string.Format("Assembly {0} has more than one BaseModPlugin, duplicates will be ignored!", assembly.FullName));
                    }
                    else
                    {
                        foundBasePlugin = true;
                        BaseModPlugin baseModPlugin = (BaseModPlugin)Activator.CreateInstance(type);

                        PluginInfo info = baseModPlugin.Info;
                        if (!baseModPlugin.PLUGIN_BepInExIgnored)
                        {
                            controlledBepInExPlugins[path] = info;
                        }
                        tempConfigPluginInformation[assembly] = new PluginConfigInformation
                        {
                            bepInPlugin = info.Metadata,
                            defaultConfigName = baseModPlugin.ENV_DefaultConfigName,
                            configStructure = baseModPlugin.ENV_ConfigStructure,
                            trimConfigNameSpaces = baseModPlugin.ENV_TrimConfigNamespaces,
                        };

                        UnityEngine.Object.DestroyImmediate(baseModPlugin);
                    }
                }
            }
            /*if (foundBasePlugin)
            {
                assembly.GrooveSharedUtilsInfo().assetFieldLocator = currentAssetFieldLocator;
            }*/
            //currentAssetFieldLocator.Clear();
            assetCollectionScannedTypes.Clear();
            
        }
        private static void FindAssetFields(Type type, Dictionary<(string, Type), FieldInfo> assetFieldLocator)
        {
            if (assetCollectionScannedTypes.Contains(type))
            {
                return;
            }
            logger.LogInfo("display case: " + type);
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                logger.LogInfo("display case field: " + fieldInfo.Name);
                assetFieldLocator.Add((fieldInfo.Name, fieldInfo.FieldType), fieldInfo);//.ToLower()
            }
            assetCollectionScannedTypes.Add(type);
            foreach(Type nestedType in type.GetNestedTypes())
            {
                FindAssetFields(nestedType, assetFieldLocator);
            }
        }
        private static void BindConfigurableAttribute(ConfigurableAttribute attribute)
        {
            Type type = attribute.target as Type;
            FieldInfo fieldInfo = attribute.target as FieldInfo;
            bool targetsType = type != null && typeof(BaseModModule).IsAssignableFrom(type) && !type.IsAbstract;
            bool targetsFieldInfo = fieldInfo != null;
            if (!targetsType && !targetsFieldInfo)
            {
                return;
            }
            if(targetsFieldInfo && !fieldInfo.IsStatic)
            {
                logger.LogWarning($"Configurable attribute targets field {fieldInfo.Name} which MUST be static!");
                return;
            }
            Assembly assembly = (type ?? fieldInfo.DeclaringType).Assembly;
            if (!tempConfigPluginInformation.TryGetValue(assembly, out PluginConfigInformation info))
            {
                return;
            }
            
            ConfigFile configFile = GSUtil.GetOrCreateConfig(attribute.configName ?? info.defaultConfigName ?? assembly.FullName, assembly, info.bepInPlugin);
            if (configFile == null || info.configStructure <= (ConfigStructure)(-1) || info.configStructure >= ConfigStructure.Count)
            {
                return;
            }
            if (targetsType)
            {
                if(!attribute.BindToType(type, configFile, info.configStructure, info.trimConfigNameSpaces))
                {
                    AssemblyInfo.Get(assembly).configDisabledModuleTypes.Add(type);
                }
            }
            else if (targetsFieldInfo)
            {
                fieldInfo.SetValue(null, attribute.BindToField(fieldInfo, configFile, info.configStructure, info.trimConfigNameSpaces));
            }
        }


        public static void Patch(AssemblyDefinition _)
        {
            
        }
    }
}
