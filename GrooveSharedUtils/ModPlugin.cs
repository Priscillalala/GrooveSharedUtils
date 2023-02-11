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
using GrooveSharedUtils.ScriptableObjects;
using System.Runtime.CompilerServices;
using GrooveSharedUtils.Interfaces;
using RoR2.Projectile;
using UnityEngine.Networking;
using System.Diagnostics;

namespace GrooveSharedUtils
{
    public abstract class ModPlugin<T> : ModPlugin where T : class
    {
        public static T instance { get; private set; }

        /*public override void Awake()
        {
            if(instance == null)
            {
                instance = this as T;
            }
            base.Awake();
        }*/
        public ModPlugin()
        {
            if (instance == null)
            {
                instance = this as T;
            }
        }
    }
    public abstract class ModPlugin : BaseUnityPlugin, IContentPackProvider
    {
        public abstract string ModName { get; }
        public abstract string AuthorName { get; }
        public abstract string VersionNumber { get; }
        public virtual string OverridePluginGUID { get; } = null;
        public virtual string[] HardDependencyStrings { get; } = Array.Empty<string>();
        public virtual string[] SoftDependencyStrings { get; } = Array.Empty<string>();
        public virtual string[] IncompatabilityStrings { get; } = Array.Empty<string>();
        public virtual string[] OverrideProcessNames { get; } = Array.Empty<string>();
        public virtual StatusFlags ModStatus { get; } = StatusFlags.Default; //attribute?
        //public virtual string ENV_RelativeAssetBundleFolder { get; } = string.Empty; //attribute
        //public virtual ExpansionDef ENV_DefaultExpansionDef { get; } = null; //attribute
        //public virtual string ENV_DefaultConfigName { get; } = null;//attribute
        //public virtual string ENV_GeneratedTokensPrefix { get; } = null;//attribute
        //public virtual ConfigStructure ENV_ConfigStructure { get; } = ConfigStructure.ModulesAsCategories;//attribute
        //public virtual bool ENV_TrimConfigNamespaces { get; } = true;//attribute
        //public virtual bool ENV_AutoSwapStubbedShaders { get; } = true;//attribute
        //public virtual (Type, string)[] ENV_AdditionalCommonTypePrefixes { get; } = Array.Empty<(Type, string)>();
        /*internal class Reservation
        {
            private static Dictionary<Assembly, Reservation> reservations = new Dictionary<Assembly, Reservation>();
            internal static Reservation Free(ModPlugin plugin)
            {
                if (reservations == null) return null;
                if (reservations.TryFreeValue(plugin.assembly, out Reservation reservation))
                {
                    if (reservations.Count <= 0)
                    {
                        reservations = null;
                    }
                    return reservation;
                }
                return new Reservation();
            }
            internal static Reservation GetOrCreate(Assembly assembly) => reservations.GetOrCreateValue(assembly);
            internal AssetFieldLocator assetFieldLocator = new AssetFieldLocator();
            //internal List<ConfigurableAttribute> configurableAttributes = new List<ConfigurableAttribute>();

        }*/
        internal static Dictionary<Assembly, ModPlugin> assemblyToPlugin = new Dictionary<Assembly, ModPlugin>();
        public static bool TryFind(Assembly assembly, out ModPlugin plugin) => assemblyToPlugin.TryGetValue(assembly, out plugin);
        public static event Action<Type, ModPlugin> onBeforeModuleInstantiated;
        //static List<Task> swapShadersTasks = new List<Task>();
        //static Dictionary<Shader, Shader> stubbedToRealShaderCache = new Dictionary<Shader, Shader>();

        /*static (Type, string)[] _globalTypeToCommonPrefix = new (Type, string)[]
        {
            (typeof(NetworkSoundEventDef), "nse"),
            (typeof(BuffDef), "bd"),
            (typeof(EliteDef), "ed"),
            (typeof(DirectorCardCategorySelection), "dccs"),
            (typeof(DccsPool), "dp"),
            (typeof(ItemDisplayRuleSet), "idrs"),
        };

        public static ReadOnlyArray<(Type, string)> globalTypeToCommonPrefix = new ReadOnlyArray<(Type, string)>(_globalTypeToCommonPrefix);
        static Dictionary<Type, HashSet<string>> typeToPossiblePrefixesCache = new Dictionary<Type, HashSet<string>>();*/
        /*static ModPlugin()
        {
            RoR2Application.onLoad = (Action)Delegate.Combine(RoR2Application.onLoad, new Action(delegate 
            {
                Task.WaitAll(swapShadersTasks.ToArray());
                swapShadersTasks = null;
                stubbedToRealShaderCache = null;
            }));
        }*/
        public GameObject moduleManagerObject;
        public string generatedGUID;
        public Assembly assembly;
        //public AssetFieldLocator assetFieldLocator;
        //public List<AssetBundle> assetBundles = new List<AssetBundle>();
        //public List<ConfigFile> configFiles = new List<ConfigFile>();
        public ContentPack contentPack = new ContentPack();
        //public HashSet<Type> configDisabledModuleTypes = new HashSet<Type>();
        public List<ModModule> moduleOrder;
        public HashSet<Type> enabledModuleTypes;
        //public string adjustedGeneratedTokensPrefix => string.IsNullOrEmpty(ENV_GeneratedTokensPrefix) ? string.Empty : ENV_GeneratedTokensPrefix.ToUpper();
        public bool isDebug => (ModStatus & StatusFlags.Debug) > 0;
        public bool isWIP => (ModStatus & StatusFlags.WIP) > 0;
        public string identifier => generatedGUID;
        [Obsolete(".Config should not be used. Refer to ConfigManager.GetOrCreate instead.", false)]
        public new ConfigFile Config
        {
            get => base.Config;
        }
        public PluginInfo SetupPluginInfo()
        {
            PluginInfo pluginInfo = new PluginInfo();
            pluginInfo.Instance = this;

            Type type = base.GetType();

            string authorName = AuthorName;
            string modName = ModName;
            string version = VersionNumber;
            generatedGUID = string.IsNullOrEmpty(OverridePluginGUID) ? string.Format("com.{0}.{1}", authorName, modName) : OverridePluginGUID;
            BepInPlugin bepInPlugin = new BepInPlugin(generatedGUID, modName, version);
            pluginInfo.Metadata = bepInPlugin;

            List<BepInDependency> bepInDependencies = new List<BepInDependency>();
            bepInDependencies.AddRange(from string hardDependency in HardDependencyStrings select new BepInDependency(hardDependency, BepInDependency.DependencyFlags.HardDependency));
            bepInDependencies.AddRange(from string softDependency in SoftDependencyStrings select new BepInDependency(softDependency, BepInDependency.DependencyFlags.SoftDependency));
            pluginInfo.Dependencies = bepInDependencies;

            pluginInfo.Processes = (from string processName in OverrideProcessNames select new BepInProcess(processName));

            pluginInfo.Incompatibilities = (from string incompat in IncompatabilityStrings select new BepInIncompatibility(incompat));

            pluginInfo.TypeName = type.FullName;

            Assembly assembly = type.Assembly;
            AssemblyName assemblyName = assembly.GetReferencedAssemblies().FirstOrDefault((AssemblyName name) => name.Name == "BepInEx");
            pluginInfo.TargettedBepInExVersion = (assemblyName?.Version) ?? Chainloader.CurrentAssemblyVersion;

            pluginInfo.Location = assembly.Location;

            return pluginInfo;
        }
        public ManualLogSource SetupLogger()
        {
            return BepInEx.Logging.Logger.CreateLogSource(ModName);
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
        public virtual IEnumerator LoadContent() => null;
        public virtual void Awake()
        {
            ContentManager.collectContentPackProviders += this.ContentManager_collectContentPackProviders;

            assembly = base.GetType().Assembly;
            /*assemblyInfo = AssemblyInfo.Get(assembly);
            assemblyInfo.plugin = this;*/
            assemblyToPlugin[assembly] = this;

            //Reservation reservation = Reservation.Free(this);
            //assetFieldLocator = reservation.assetFieldLocator;
            /*foreach (ConfigurableAttribute attribute in reservation.configurableAttributes)
            {
                TryBindConfigurableAttribute(attribute);
            }*/

            /*IEnumerable<ConfigurableAttribute> configurableAttributes =  List<ConfigurableAttribute> configurableAttributes = new List<ConfigurableAttribute>();
            HG.Reflection.SearchableAttribute.GetInstances(configurableAttributes);
            foreach (ConfigurableAttribute attribute in configurableAttributes)
            {
                BindConfigurableAttribute(attribute);
            }*/

            moduleManagerObject = new GameObject(ModName + "_ModuleManager");
            DontDestroyOnLoad(moduleManagerObject);

            /*while (assemblyInfo.pendingDisplayAssets.Count > 0)
            {
                AddDisplayAsset(assemblyInfo.pendingDisplayAssets.Dequeue());
            }*/
            /*if (ENV_RelativeAssetBundleFolder != null)
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
            }*/

            Type baseModuleType = typeof(ModModule);
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
                && (isDebug || type.GetCustomAttribute<DebugModuleAttribute>() == null)
                && (isWIP || type.GetCustomAttribute<WIPModuleAttribute>() == null)
                && ConfigurableAttribute.CheckModuleTypeEnabled(type, Info);
        }
        public virtual ModModule CreateModule(Type type)
        {
            if (isDebug) 
            {
                Logger.Log(LogLevel.Debug, "Creating Module: " + type.Name);
            }
            onBeforeModuleInstantiated?.Invoke(type, this);
            ModModule baseModule = null;
            try
            {
                ModModule.earlyAssignmentOwner = this;
                baseModule = (ModModule)moduleManagerObject.AddComponent(type);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.ToString());
            }
            finally
            {
                ModModule.earlyAssignmentOwner = null;
            }
            return baseModule;
        }
        public bool ModuleEnabled<T>() where T : ModModule => ModuleEnabled(typeof(T));
        public virtual bool ModuleEnabled(Type type) => enabledModuleTypes.Contains(type);
        public virtual string GetAssetName(object asset) => AssetDisplayCaseAttribute.GetAssetName(asset);
        public virtual AssetToContentMap GetAssetToContentMap()
        {
            AssetToContentMap map = new AssetToContentMap(contentPack);
            map = new AssetToContentMap(contentPack);
            map[typeof(GameObject)] = (object obj) =>
            {
                GameObject g = (GameObject)obj;
                if (g.GetComponent<ProjectileController>())
                {
                    map.PlanAdd(contentPack.projectilePrefabs, g);
                }
                else if (g.GetComponent<CharacterMaster>())
                {
                    map.PlanAdd(contentPack.masterPrefabs, g);
                }
                else if (g.GetComponent<CharacterBody>())
                {
                    map.PlanAdd(contentPack.bodyPrefabs, g);
                }
                else if (g.GetComponent<Run>())
                {
                    map.PlanAdd(contentPack.gameModePrefabs, g);
                }
                else if (g.GetComponent<NetworkIdentity>())
                {
                    map.PlanAdd(contentPack.networkedObjectPrefabs, g);
                }
            };
            map[typeof(IRegisterable)] = (object obj) =>
            {
                ((IRegisterable)obj).Register();
            };
            map[typeof(Type)] = (object obj) =>
            {
                Type t = (Type)obj;
                if (t.IsSubclassOf(typeof(EntityStates.EntityState)))
                {
                    map.PlanAdd(contentPack.entityStateTypes, t);
                }
            };
            map[typeof(AchievementDef)] = (object obj) =>
            {
                EarlyAchievementManager.Add((AchievementDef)obj);
            };
            if (GSUtil.ModLoaded("com.bepis.r2api.items"))
            {
                AddItemAPIToMap(map);
            }
            if (GSUtil.ModLoaded("com.bepis.r2api.colors"))
            {
                AddColorAPIToMap(map);
            }
            if (GSUtil.ModLoaded("com.bepis.r2api.artifactcode"))
            {
                AddArtifactCodeAPIToMap(map);
            }
            return map;
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal void AddItemAPIToMap(AssetToContentMap map)
        {
            map[typeof(CustomItem)] = (object obj) =>
            {
                ItemAPI.Add((CustomItem)obj);
            };
            map[typeof(CustomEquipment)] = (object obj) =>
            {
                ItemAPI.Add((CustomEquipment)obj);
            };
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal void AddColorAPIToMap(AssetToContentMap map)
        {
            map[typeof(SerializableDamageColor)] = (object obj) =>
            {
                ColorsAPI.AddSerializableDamageColor((SerializableDamageColor)obj);
            };
            map[typeof(SerializableColorCatalogEntry)] = (object obj) =>
            {
                ColorsAPI.AddSerializableColor((SerializableColorCatalogEntry)obj);
            };
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal void AddArtifactCodeAPIToMap(AssetToContentMap map)
        {
            map[typeof(ArtifactCompoundDef)] = (object obj) =>
            {
                ArtifactCodeAPI.AddCompound((ArtifactCompoundDef)obj);
            };
        }
        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public virtual IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = identifier;
            AssetToContentMap map = GetAssetToContentMap();

            IEnumerator pluginContent = LoadContent();
            if (pluginContent != null)
            {
                yield return new CollectContent(pluginContent, this, map);
            }

            float progressPerModule = 1f / moduleOrder.Count();
            float progress = 0f;
            foreach (ModModule module in moduleOrder)
            {
                IEnumerator moduleContent = module.LoadContent();
                if (moduleContent != null)
                {
                    yield return new CollectContent(moduleContent, this, map);
                }
                progress += progressPerModule;
                args.ReportProgress(progress);
            }

            map.ResolveAllPlans();
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

        public class CollectContent : IEnumerator
        {
            private Stack<IEnumerator> iterators = new Stack<IEnumerator>();
            private ModPlugin plugin;
            private AssetToContentMap map;
            public CollectContent(IEnumerator content, ModPlugin plugin, AssetToContentMap map) 
            {
                this.iterators.Push(content);
                this.plugin = plugin;
                this.map = map;
            }
            public object Current => null;
            public bool MoveNext()
            {
                try 
                {
                    IEnumerator enumerator = iterators.Peek();
                    if (enumerator.MoveNext()) 
                    {
                        //plugin.Logger.LogWarning($"Move Next! current is {enumerator.Current}");
                        if (enumerator.Current is IEnumerator iterator)
                        {
                            iterators.Push(iterator);
                        }
                        else 
                        {
                            Collect(enumerator.Current);
                        }
                    }
                    else
                    {
                        iterators.Pop();
                    }
                }
                catch (Exception ex)
                {
                    plugin.Logger.Log(LogLevel.Error, ex.ToString());
                }
                return iterators.Count > 0;
            }
            public void Collect(object asset) 
            {
                if (asset is IEnumerable enumerable)
                {
                    foreach (object obj in enumerable)
                    {
                        Collect(obj);
                    }
                    return;
                }
                AssetDisplayCaseAttribute.TryDisplayAsset(asset, plugin.GetAssetName(asset), plugin.assembly);
                map.TryMapAsset(asset);
                if (GSUtil.ModLoadedCached("com.bepis.r2api.content_management") && CheckForR2APISerializableContentPack(asset))
                {
                    return;
                }
                if (asset is GameObject gameObject && gameObject.GetComponent<EffectComponent>())
                {
                    Collect(new EffectDef(gameObject));
                    return;
                }
            }
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal bool CheckForR2APISerializableContentPack(object asset)
            {
                if (asset is R2APISerializableContentPack serializableContentPack)
                {
                    FieldInfo[] fields = serializableContentPack.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        FieldInfo fieldInfo = fields[i];
                        if (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
                        {
                            Collect(fieldInfo.GetValue(serializableContentPack));
                        }
                    }
                    return true;
                }
                return false;
            }
            public void Reset()
            {
                while (iterators.Count > 0)
                {
                    iterators.Pop();
                }
            }
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
/*public virtual void TryBindConfigurableAttribute(ConfigurableAttribute attribute)
        {
            if (attribute.targetsType)
            {
                TryBindConfigurableAttributeToType(attribute, attribute.target as Type);
            }
            else if (attribute.targetsField)
            {
                TryBindConfigurableAttributeToField(attribute, attribute.target as FieldInfo);
            }
        }
        public virtual void TryBindConfigurableAttributeToType(ConfigurableAttribute attribute, Type target)
        {
            if (!target.IsSubclassOf(typeof(ModModule)))
            {
                Logger.LogWarning($"Configurable attribute targets type {target.Name} which does not inherit from {nameof(ModModule)}!");
                return;
            }
            ConfigFile config = ConfigManager.GetOrCreate(attribute.configName ?? ENV_DefaultConfigName ?? generatedGUID, assembly, Info.Metadata);
            if (config != null)
            {
                if(!attribute.BindToType(target, config, ENV_ConfigStructure, ENV_TrimConfigNamespaces, out ConfigEntryBase configEntry))
                {
                    configDisabledModuleTypes.Add(target);
                }
                AddDisplayAsset(configEntry);
            }
        }
        public virtual void TryBindConfigurableAttributeToField(ConfigurableAttribute attribute, FieldInfo target)
        {
            if (!target.IsStatic)
            {
                Logger.LogWarning($"Configurable attribute targets field {target.Name} which MUST be static!");
                return;
            }
            ConfigFile config = ConfigManager.GetOrCreate(attribute.configName ?? ENV_DefaultConfigName ?? generatedGUID, assembly, Info.Metadata);
            if (config != null)
            {
                target.SetValue(null, attribute.BindToField(target, config, ENV_ConfigStructure, ENV_TrimConfigNamespaces, out ConfigEntryBase configEntry));
                AddDisplayAsset(configEntry);
            }
        }*/

/*public async Task SwapStubbedShaders(AssetBundle assetBundle)
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
                    if(ENV_AdditionalStubbedShaderPairs.TryGetValue(name, out Shader fromEnvShader))
                    {
                        return fromEnvShader;
                    }
                    if (globalStubbedShaderPairs.TryGetValue(name, out Shader fromGlobalShader))
                    {
                        return fromGlobalShader;
                    }
                    return null;
                });
                GSUtil.Log(shader != null);
                mat.shader = shader ?? mat.shader;
                GSUtil.Log("Final Shader: " +  mat.shader.name);
            }
        };
    });
}*/
/*public virtual void AddDisplayAsset(object asset)
{
    if (asset == null) return;
    Type assetType = asset.GetType();
    string assetName = GetAssetName(asset);
    if (string.IsNullOrEmpty(assetName)) return;
    //assetName = assetName.FormatCharacters((char c) => !char.IsWhiteSpace(c));
    if (assetFieldLocator.TryGetValue((assetName, assetType), out FieldInfo field))
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
            if (assetFieldLocator.TryGetValue((assetName.Substring(commonPrefix.Length), assetType), out FieldInfo fieldFromPrefix))
            {
                fieldFromPrefix.SetValue(null, asset);
            }
        }
    }
}*/
