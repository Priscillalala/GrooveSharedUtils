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
using R2API.Utils;
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

namespace GrooveSharedUtils
{
    public abstract class BaseModPlugin<T> : BaseModPlugin where T : class
    {
        public static T instance { get; private set; }
        public BaseModPlugin()
        {
            if (instance != null)
            {
                return;
            }
            instance = this as T;
        }
    }
    public abstract class BaseModPlugin : BaseUnityPlugin, IContentPackProvider
    {
        public abstract string PLUGIN_ModName { get; }
        public abstract string PLUGIN_AuthorName { get; }
        public abstract string PLUGIN_VersionNumber { get; }
        public virtual string PLUGIN_OverrideModPrefix { get; } = null;
        public virtual string[] PLUGIN_HardDependencyStrings { get; } = Array.Empty<string>();
        public virtual string[] PLUGIN_SoftDependencyStrings { get; } = Array.Empty<string>();
        public virtual string[] PLUGIN_IncompatabilityStrings { get; } = Array.Empty<string>();
        public virtual string[] PLUGIN_OverrideProcessNames { get; } = Array.Empty<string>();
        public virtual string ENVIRONMENT_OverrideAssetBundleFolder { get; } = null;
        public virtual ExpansionDef ENVIRONMENT_DefaultExpansionDef { get; } = null;
        public virtual string ENVIRONMENT_DefaultConfigName { get; } = null;
        public virtual string ENVIRONMENT_GeneratedTokensPrefix { get; } = null;
        public virtual ConfigStructure ENVIRONMENT_ConfigStructure { get; } = ConfigStructure.ModulesAsCategories;
        public virtual bool ENVIRONMENT_TrimConfigNamespaces { get; } = true;
        public virtual bool ENVIRONMENT_AutoSwapStubbedShaders { get; } = true;
        public virtual Dictionary<Type, string> ENVIRONMENT_AdditionalCommonTypePrefixes { get; } = new Dictionary<Type, string>();
        public virtual Dictionary<string, Shader> ENVIRONMENT_AdditionalStubbedShaderPairs { get; } = new Dictionary<string, Shader>();
        public string identifier => generatedGUID;
        public string generatedGUID;
        public Assembly assembly;
        public List<AssetBundle> assetBundles = new List<AssetBundle>();
        public List<ConfigFile> configFiles = new List<ConfigFile>();
        public ContentPack contentPack = new ContentPack();
        public List<BaseModModule> moduleOrder;
        public string getGeneratedTokensPrefix
        {
            get
            {
                return string.IsNullOrEmpty(ENVIRONMENT_GeneratedTokensPrefix) ? string.Empty : ENVIRONMENT_GeneratedTokensPrefix.ToUpper();
            }
        }

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
            string prefix = string.IsNullOrEmpty(PLUGIN_OverrideModPrefix) ? "com" : PLUGIN_OverrideModPrefix;
            generatedGUID = string.Format("{0}.{1}.{2}", prefix, authorName, modName);
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
            GrooveSharedUtilsPlugin.AssemblyInfo info = GrooveSharedUtilsPlugin.GetAssemblyInfo(assembly);
            info.plugin = this;

            foreach (object asset in info.pendingDisplayAssets)
            {
                AddDisplayAsset(asset);
            }
            string directoryPath = System.IO.Path.GetDirectoryName(assembly.Location);
            if (!string.IsNullOrEmpty(ENVIRONMENT_OverrideAssetBundleFolder))
            {
                directoryPath = System.IO.Path.Combine(directoryPath, ENVIRONMENT_OverrideAssetBundleFolder);
            }
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
                            if (ENVIRONMENT_AutoSwapStubbedShaders)
                            {
                                Task t = SwapStubbedShaders(assetBundle);
                            }
                        }
                    }
                }
                catch { }
            }

            BeginModInit();

            Type baseModuleType = typeof(BaseModModule);
            List<Type> orderedModulesTypes = assembly.GetTypes().Where((Type t) => t.IsSubclassOf(baseModuleType) && !t.IsAbstract && IsModuleEnabled(t)).OrderByDescending(new Func<Type, int>(GetModuleTypePriority)).ToList();
            moduleOrder = orderedModulesTypes.Select((Type t) => CreateModule(t)).ToList(); //(from t in orderedModulesTypes select CreateModule(t));
        }
        public virtual int GetModuleTypePriority(Type t)
        {
            ModuleOrderPriorityAttribute moduleLoadPriorityAttribute = t.GetCustomAttribute<ModuleOrderPriorityAttribute>();
            return moduleLoadPriorityAttribute != null ? moduleLoadPriorityAttribute.priority : 0;
        }
        public virtual bool IsModuleEnabled(Type type)
        {
            return type.GetCustomAttribute<IgnoreModuleAttribute>() == null && !GrooveSharedUtilsPlugin.configDisabledModuleTypes.Contains(type);
            
        }
        public virtual BaseModModule CreateModule(Type type)
        {
            GSUtil.Log("creating module: " + type.Name);
            BaseModModule baseModule = (BaseModModule)Activator.CreateInstance(type);
            try
            {
                baseModule.OnModInit();
            }
            catch(Exception ex)
            {
                GSUtil.Log(LogLevel.Error, ex.ToString());
            }
            return baseModule;
        }
        public virtual void AddDisplayAsset<TAsset>(TAsset asset)
        {
            Type assetType = asset.GetType();
            GSUtil.Log("try map asset: " + asset);
            string assetName = GetAssetName(asset);
            if (string.IsNullOrEmpty(assetName))
            {
                return;
            }
            assetName = assetName.FormatCharacters((char c) => !char.IsWhiteSpace(c));
            if (GrooveSharedUtilsPlugin.GetAssemblyInfo(assembly).assetFieldLocator.TryGetValue((assetName, assetType), out FieldInfo field))
            {
                field.SetValue(null, asset);
            }
            HashSet<string> commonPrefixes = GrooveSharedUtilsPlugin.typeToPossiblePrefixesCache.GetOrCreateValue(assetType, () => 
            {
                HashSet<string> prefixes = new HashSet<string>();
                foreach(KeyValuePair<Type, string> pair in GrooveSharedUtilsPlugin.globalTypeToCommonPrefix)
                {
                    if (pair.Key.IsAssignableFrom(assetType))
                    {
                        prefixes.Add(pair.Value);
                    }
                }
                foreach (KeyValuePair<Type, string> pair in ENVIRONMENT_AdditionalCommonTypePrefixes)
                {
                    if (pair.Key.IsAssignableFrom(assetType))
                    {
                        prefixes.Add(pair.Value);
                    }
                }
                return prefixes;
            });
            foreach(string commonPrefix in commonPrefixes)
            {
                if (assetName.StartsWith(commonPrefix))
                {
                    if (GrooveSharedUtilsPlugin.GetAssemblyInfo(assembly).assetFieldLocator.TryGetValue((assetName.Remove(0, commonPrefix.Length), assetType), out FieldInfo fieldFromPrefix))
                    {
                        fieldFromPrefix.SetValue(null, asset);
                    }
                }
            }


        }
        public virtual string GetAssetName<TAsset>(TAsset asset)
        {
            if (asset is Type t)
            {
                return t.FullName;
            }
            if (asset is EffectDef effectDef)
            {
                return effectDef.prefabName;
            }
            if (asset is Component c)
            {
                return c.gameObject.name;
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
            await Task.Run((Action)(() => 
            {
                AssetBundleRequest request = assetBundle.LoadAllAssetsAsync<Material>();
                request.completed += (operation) =>
                {
                    int length = request.allAssets.Length;
                    GSUtil.LogInternal(LogLevel.Info, string.Format("Swapping stubbed shaders for {0} materials", length), assembly);
                    Dictionary<Shader, Shader> stubbedToRealShaderCache = new Dictionary<Shader, Shader>();
                    for(int i = 0; i < length; i++)
                    {
                        Material mat = (Material)request.allAssets[i];
                        Shader shader = stubbedToRealShaderCache.GetOrCreateValue(mat.shader, (Func<Shader>)(() =>
                        {
                            string name = mat.shader.name;
                            GSUtil.Log(name);
                            if(ENVIRONMENT_AdditionalStubbedShaderPairs.TryGetValue(name, out Shader fromEnvShader))
                            {
                                return fromEnvShader;
                            }
                            if (GrooveSharedUtilsPlugin.globalStubbedShaderPairs.TryGetValue(name, out Shader fromGlobalShader))
                            {
                                return fromGlobalShader;
                            }
                            return null;
                        }));
                        if (shader != null)
                        {
                            mat.shader = shader;
                        }
                    }
                    stubbedToRealShaderCache = null;
                };
            }));
        }
    }
}
