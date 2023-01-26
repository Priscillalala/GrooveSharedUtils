using BepInEx;
using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using MonoMod.Cil;
using BepInEx.Bootstrap;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using BepInEx.Configuration;
using System.Collections.ObjectModel;
using RoR2;
using R2API;
using GrooveSharedUtils.Attributes;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Unity.Jobs;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using UnityEngine.ResourceManagement.AsyncOperations;
using HG;
using R2API.ScriptableObjects;

namespace GrooveSharedUtils
{
    public abstract class BaseModPlugin<T> : BaseModPlugin where T : class
    {
        public static T instance { get; private set; }

        public override void Awake()
        {
            if(instance == null)
            {
                instance = this as T;
            }
            base.Awake();
        }
    }
    public abstract class BaseModPlugin : BaseUnityPlugin, IContentPackProvider
    {
        static List<Task> swapShadersTasks = new List<Task>();
        static Dictionary<Shader, Shader> stubbedToRealShaderCache = new Dictionary<Shader, Shader>();

        static (Type, string)[] _globalTypeToCommonPrefix = new (Type, string)[]
        {
            (typeof(NetworkSoundEventDef), "nse"),
            (typeof(BuffDef), "bd"),
            (typeof(EliteDef), "ed"),
            (typeof(DirectorCardCategorySelection), "dccs"),
            (typeof(DccsPool), "dp"),
            (typeof(ItemDisplayRuleSet), "idrs"),
        };

        public static ReadOnlyArray<(Type, string)> globalTypeToCommonPrefix = new ReadOnlyArray<(Type, string)>(_globalTypeToCommonPrefix);
        static BaseModPlugin()
        {
            RoR2Application.onLoad = (Action)Delegate.Combine(RoR2Application.onLoad, new Action(delegate 
            {
                Task.WaitAll(swapShadersTasks.ToArray());
                swapShadersTasks = null;
                stubbedToRealShaderCache = null;
            }));
        }
        static Dictionary<Type, HashSet<string>> typeToPossiblePrefixesCache = new Dictionary<Type, HashSet<string>>();
        public abstract string PLUGIN_ModName { get; }
        public abstract string PLUGIN_AuthorName { get; }
        public abstract string PLUGIN_VersionNumber { get; }
        public virtual string PLUGIN_OverridePluginGUID { get; } = null;
        public virtual bool PLUGIN_BepInExIgnored { get; } = false;
        public virtual string[] PLUGIN_HardDependencyStrings { get; } = Array.Empty<string>();
        public virtual string[] PLUGIN_SoftDependencyStrings { get; } = Array.Empty<string>();
        public virtual string[] PLUGIN_IncompatabilityStrings { get; } = Array.Empty<string>();
        public virtual string[] PLUGIN_OverrideProcessNames { get; } = Array.Empty<string>();
        public virtual StatusFlags ENV_ModStatus { get; } = StatusFlags.Default;
        public virtual string ENV_RelativeAssetBundleFolder { get; } = string.Empty;
        public virtual ExpansionDef ENV_DefaultExpansionDef { get; } = null;
        public virtual string ENV_DefaultConfigName { get; } = null;
        public virtual string ENV_GeneratedTokensPrefix { get; } = null;
        public virtual ConfigStructure ENV_ConfigStructure { get; } = ConfigStructure.ModulesAsCategories;
        public virtual bool ENV_TrimConfigNamespaces { get; } = true;
        public virtual bool ENV_AutoSwapStubbedShaders { get; } = true;
        public virtual (Type, string)[] ENV_AdditionalCommonTypePrefixes { get; } = Array.Empty<(Type, string)>();
        public string identifier => generatedGUID;
        public GameObject moduleManagerObject;
        public string generatedGUID;
        public Assembly assembly;
        public AssemblyInfo assemblyInfo;
        public List<AssetBundle> assetBundles = new List<AssetBundle>();
        public List<ConfigFile> configFiles = new List<ConfigFile>();
        public ContentPack contentPack = new ContentPack();
        public List<BaseModModule> moduleOrder;
        public HashSet<Type> enabledModuleTypes;
        public string adjustedGeneratedTokensPrefix => string.IsNullOrEmpty(ENV_GeneratedTokensPrefix) ? string.Empty : ENV_GeneratedTokensPrefix.ToUpper();
        public bool isDebug => (ENV_ModStatus & StatusFlags.Debug) > 0;
        public bool isWIP => (ENV_ModStatus & StatusFlags.WIP) > 0;
        [Obsolete(".Config should not be used. Refer to GSUtil.GetOrCreateConfig instead.", false)]
        public new ConfigFile Config
        {
            get => base.Config;
        }
        public PluginInfo SetupPluginInfo()
        {
            PluginInfo pluginInfo = new PluginInfo();
            pluginInfo.Instance = this;
            
            Type type = base.GetType();

            string authorName = PLUGIN_AuthorName.ToLower();
            string modName = PLUGIN_ModName;
            string version = PLUGIN_VersionNumber;
            generatedGUID = string.IsNullOrEmpty(PLUGIN_OverridePluginGUID) ? string.Format("com.{0}.{1}", authorName, modName) : PLUGIN_OverridePluginGUID;
            BepInPlugin bepInPlugin = new BepInPlugin(generatedGUID, modName, version);
            pluginInfo.Metadata = bepInPlugin;

            List<BepInDependency> bepInDependencies = new List<BepInDependency>();
            bepInDependencies.AddRange(from string hardDependency in PLUGIN_HardDependencyStrings select new BepInDependency(hardDependency, BepInDependency.DependencyFlags.HardDependency));
            bepInDependencies.AddRange(from string softDependency in PLUGIN_SoftDependencyStrings select new BepInDependency(softDependency, BepInDependency.DependencyFlags.SoftDependency));
            pluginInfo.Dependencies = bepInDependencies;

            pluginInfo.Processes = (from string processName in PLUGIN_OverrideProcessNames select new BepInProcess(processName));

            pluginInfo.Incompatibilities = (from string incompat in PLUGIN_IncompatabilityStrings select new BepInIncompatibility(incompat));

            pluginInfo.TypeName = type.FullName;

            Assembly assembly = type.Assembly;
            AssemblyName assemblyName = assembly.GetReferencedAssemblies().FirstOrDefault((AssemblyName name) => name.Name == "BepInEx");
            pluginInfo.TargettedBepInExVersion = (assemblyName != null ? assemblyName.Version : null) ?? new Version();

            pluginInfo.Location = assembly.Location;

            return pluginInfo;
        }
        public ManualLogSource SetupLogger()
        {
            return BepInEx.Logging.Logger.CreateLogSource(PLUGIN_ModName);
        }
        public ConfigFile SetupConfigFile()
        {
            string text = Chainloader.IsEditor ? "." : Paths.ConfigPath;
            return new ConfigFile(Utility.CombinePaths(new string[]
            {
                text,
                Info.Metadata.GUID + ".cfg"
            }), false, Info.Metadata);
        }
        public abstract void BeginModInit();
        public abstract void BeginCollectContent(AssetStream sasset);
        public virtual void Awake()
        {
            ContentManager.collectContentPackProviders += this.ContentManager_collectContentPackProviders;

            assembly = base.GetType().Assembly;
            assemblyInfo = AssemblyInfo.Get(assembly);
            assemblyInfo.plugin = this;

            moduleManagerObject = new GameObject(PLUGIN_ModName + "_ModuleManager");
            DontDestroyOnLoad(moduleManagerObject);

            while(assemblyInfo.pendingDisplayAssets.Count > 0)
            {
                AddDisplayAsset(assemblyInfo.pendingDisplayAssets.Dequeue());
            }
            if (ENV_RelativeAssetBundleFolder != null)
            {
                string directoryPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), ENV_RelativeAssetBundleFolder);
                foreach (string path in Directory.EnumerateFiles(directoryPath))
                {
                    try
                    {
                        string extension = System.IO.Path.GetExtension(path);
                        if (string.IsNullOrEmpty(extension) || extension == "bundle")
                        {
                            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
                            if (assetBundle)
                            {
                                assetBundles.Add(assetBundle);
                                AddDisplayAsset(assetBundle);
                                if (ENV_AutoSwapStubbedShaders)
                                {
                                    Task t = SwapStubbedShaders(assetBundle);
                                    swapShadersTasks.Add(t);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            BeginModInit();

            Type baseModuleType = typeof(BaseModModule);
            List<Type> orderedModulesTypes = assembly.GetTypes().Where((Type t) => t.IsSubclassOf(baseModuleType) && !t.IsAbstract && ShouldEnableModuleType(t)).OrderByDescending(new Func<Type, int>(GetModuleTypePriority)).ToList();
            enabledModuleTypes = new HashSet<Type>(orderedModulesTypes);
            moduleOrder = orderedModulesTypes.Select((Type t) => CreateModule(t)).ToList(); //(from t in orderedModulesTypes select CreateModule(t));
        }
        public virtual int GetModuleTypePriority(Type t)
        {
            ModuleOrderPriorityAttribute moduleLoadPriorityAttribute = t.GetCustomAttribute<ModuleOrderPriorityAttribute>();
            return moduleLoadPriorityAttribute != null ? moduleLoadPriorityAttribute.priority : 0;
        }
        public virtual bool ShouldEnableModuleType(Type type)
        {
            return type.GetCustomAttribute<IgnoreModuleAttribute>() == null
                && !assemblyInfo.configDisabledModuleTypes.Contains(type)
                && (isDebug || type.GetCustomAttribute<DebugModuleAttribute>() == null)
                && (isWIP || type.GetCustomAttribute<WIPModuleAttribute>() == null);
        }
        public virtual BaseModModule CreateModule(Type type)
        {
            GSUtil.LogDebug(LogLevel.Debug, "Creating Module: " + type.Name, assemblyInfo);
            BaseModModule baseModule = (BaseModModule)moduleManagerObject.AddComponent(type);
            try
            {
                baseModule.OnModInit();
            }
            catch (Exception ex)
            {
                GSUtil.Log(LogLevel.Error, ex.ToString());
            }
            return baseModule;
        }
        public bool ModuleEnabled<T>() where T:BaseModModule => ModuleEnabled(typeof(T));
        public virtual bool ModuleEnabled(Type type) => enabledModuleTypes.Contains(type);
        public virtual void AddDisplayAsset(object asset)
        {
            if(asset == null)
            {
                return;
            }
            Type assetType = asset.GetType();
            string assetName = GetAssetName(asset);
            if (string.IsNullOrEmpty(assetName))
            {
                return;
            }
            //assetName = assetName.FormatCharacters((char c) => !char.IsWhiteSpace(c));
            if (AssemblyInfo.Get(assembly).assetFieldLocator.TryGetValue((assetName, assetType), out FieldInfo field))
            {
                field.SetValue(null, asset);
            }
            HashSet<string> commonPrefixes = typeToPossiblePrefixesCache.GetOrCreateValue(assetType, () => 
            {
                HashSet<string> prefixes = new HashSet<string>();
                foreach((Type, string) pair in globalTypeToCommonPrefix)
                {
                    if (pair.Item1.IsAssignableFrom(assetType))
                    {
                        prefixes.Add(pair.Item2);
                    }
                }
                foreach ((Type, string) pair in ENV_AdditionalCommonTypePrefixes)
                {
                    if (pair.Item1.IsAssignableFrom(assetType))
                    {
                        prefixes.Add(pair.Item2);
                    }
                }
                return prefixes;
            });
            foreach(string commonPrefix in commonPrefixes)
            {
                if (assetName.StartsWith(commonPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (AssemblyInfo.Get(assembly).assetFieldLocator.TryGetValue((assetName.Substring(commonPrefix.Length), assetType), out FieldInfo fieldFromPrefix))
                    {
                        fieldFromPrefix.SetValue(null, asset);
                    }
                }
            }
        }
        public virtual string GetAssetName(object asset)
        {
            if (asset is Type t)
            {
                return t.Name;
            }
            if (asset is EffectDef effectDef)
            {
                return effectDef.prefabName;
            }
            if (asset is Component c)
            {
                return c.gameObject.name;
            }
            if(asset is SkillDef sd && !string.IsNullOrEmpty(sd.skillName))
            {
                return sd.skillName;
            }
            if (asset is UnityEngine.Object obj)
            {
                return obj.name;
            }
            if (asset is ConfigFile configFile)
            {
                return System.IO.Path.GetFileNameWithoutExtension(configFile.ConfigFilePath);
            }
            return null;
        }
        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public virtual IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = identifier;
            AssetStream assetStream = new AssetStream(this);
            assetStream.GenerateMap();

            BeginCollectContent(assetStream);

            float progressPerModule = 1f / moduleOrder.Count();
            float progress = 0f;
            foreach (BaseModModule module in moduleOrder)
            {
                try
                {
                    module.OnCollectContent(assetStream);
                }
                catch (Exception ex)
                {
                    GSUtil.Log(LogLevel.Error, ex.ToString());
                }
                
                progress += progressPerModule;
                args.ReportProgress(progress);
            }

            assetStream.ResolveMap();
            args.ReportProgress(1f);
            yield break;
        }

        public virtual IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public virtual IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
        public async Task SwapStubbedShaders(AssetBundle assetBundle)
        {
            await Task.Run(() => 
            {
                AssetBundleRequest request = assetBundle.LoadAllAssetsAsync<Material>();
                request.completed += (operation) =>
                {
                    int length = request.allAssets.Length;
                    Logger.LogInfo($"Swapping stubbed shaders for {length} materials from {assetBundle.name}");
                    for(int i = 0; i < length; i++)
                    {
                        Material mat = (Material)request.allAssets[i];
                        Shader shader = stubbedToRealShaderCache.GetOrCreateValue(mat.shader, () =>
                        {
                            Shader realShader = null;
                            string name = mat.shader.name;
                            if (name.StartsWith("Stubbed"))
                            {
                                AsyncOperationHandle<Shader> asyncOperationHandle = default;

                                string path = name.Substring(7) + ".shader";
                                GSUtil.Log(name);
                                GSUtil.Log(path);
                                try
                                {
                                    asyncOperationHandle = Addressables.LoadAssetAsync<Shader>(path);
                                }
                                finally
                                {
                                    if (asyncOperationHandle.IsValid())
                                    {
                                        GSUtil.Log("Valid!");
                                        realShader = asyncOperationHandle.WaitForCompletion();
                                    }                                    
                                }
                            }
                            GSUtil.Log(realShader != null);
                            return realShader;
                            /*if(ENV_AdditionalStubbedShaderPairs.TryGetValue(name, out Shader fromEnvShader))
                            {
                                return fromEnvShader;
                            }
                            if (globalStubbedShaderPairs.TryGetValue(name, out Shader fromGlobalShader))
                            {
                                return fromGlobalShader;
                            }
                            return null;*/
                        });
                        GSUtil.Log(shader != null);
                        mat.shader = shader ?? mat.shader;
                        GSUtil.Log("Final Shader: " +  mat.shader.name);
                    }
                };
            });
        }
    }
}
//static Dictionary<string, Shader> _globalStubbedShaderPairs = new Dictionary<string, Shader>();
//public static ReadOnlyDictionary<string, Shader> globalStubbedShaderPairs = new ReadOnlyDictionary<string, Shader>(_globalStubbedShaderPairs);

/*_globalStubbedShaderPairs["StubbedShader/deferred/standard"] = Common.Shaders.standard;
_globalStubbedShaderPairs["StubbedShader/fx/hgcloudremap"] = Common.Shaders.cloudRemap;
_globalStubbedShaderPairs["StubbedShader/fx/hgopaquecloudremap"] = Common.Shaders.opaqueCloudRemap;
_globalStubbedShaderPairs["StubbedShader/fx/cloudintersectionremap"] = Common.Shaders.intersectionCloudRemap;
_globalStubbedShaderPairs["StubbedShader/fx/hgdistortion"] = Common.Shaders.distortion;
_globalStubbedShaderPairs["StubbedShader/fx/solidparallax"] = Common.Shaders.solidParallax;
_globalStubbedShaderPairs["StubbedShader/fx/hgforwardplanet"] = Common.Shaders.forwardPlanet;
_globalStubbedShaderPairs["StubbedShader/fx/distantwater"] = Common.Shaders.distantWater;
_globalStubbedShaderPairs["StubbedShader/fx/hgtriplanarterrainblend"] = Common.Shaders.triplanarTerrain;
_globalStubbedShaderPairs["StubbedShader/fx/hgsnowtopped"] = Common.Shaders.snowTopped;
_globalTypeToCommonPrefix[typeof(NetworkSoundEventDef)] = "nse";
_globalTypeToCommonPrefix[typeof(BuffDef)] = "bd";
_globalTypeToCommonPrefix[typeof(EliteDef)] = "ed";
_globalTypeToCommonPrefix[typeof(DirectorCardCategorySelection)] = "dccs";
_globalTypeToCommonPrefix[typeof(DccsPool)] = "dp";
_globalTypeToCommonPrefix[typeof(ItemDisplayRuleSet)] = "idrs";*/
