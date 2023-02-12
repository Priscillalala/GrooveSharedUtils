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
using BepInEx.Configuration;
using System.Linq;
using BepInEx.Logging;
using GrooveSharedUtils.Attributes;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Collections;

namespace GrooveSharedUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class LoadAssetBundleAttribute : HG.Reflection.SearchableAttribute
    {
        public string bundleName = null;
        public string relativeFolder = null;
        public bool swapStubbedShaders = true;

        internal static void PatcherInit()
        {
            SettingsAttribute defaultSettings = new SettingsAttribute();
            Dictionary<Assembly, SettingsAttribute> settingsCache = new Dictionary<Assembly, SettingsAttribute>();

            List<LoadAssetBundleAttribute> loadAssetBundleAttributes = new List<LoadAssetBundleAttribute>();
            GetInstances(loadAssetBundleAttributes);

            List<AssetBundle> swapShadersBundles = new List<AssetBundle>();

            foreach (LoadAssetBundleAttribute attribute in loadAssetBundleAttributes)
            {
                if (attribute.target is FieldInfo fieldInfo)
                {
                    Assembly assembly = fieldInfo.DeclaringType.Assembly;
                    if (!fieldInfo.IsStatic)
                    {
                        GrooveSUPatcher.logger.LogWarning($"Load Asset Bundle attribute targets field {fieldInfo.Name} which MUST be static!");
                        continue;
                    }
                    if (!typeof(AssetBundle).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        GrooveSUPatcher.logger.LogWarning($"Load Asset Bundle attribute targets field {fieldInfo.Name} which MUST be of type {typeof(AssetBundle).Name}!");
                        continue;
                    }
                    SettingsAttribute settings = settingsCache.GetOrCreateValue(assembly, () => assembly.GetCustomAttribute<SettingsAttribute>() ?? defaultSettings);
                    string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), attribute.relativeFolder ?? settings.defaultRelativeFolder, attribute.bundleName ?? fieldInfo.Name);
                    AssetBundle bundle = null;
                    try 
                    {
                        bundle = AssetBundle.LoadFromFile(path);
                    }
                    catch (Exception ex)
                    {
                        GrooveSUPatcher.logger.LogError(ex.ToString());
                    }
                    if (bundle != null)
                    {
                        fieldInfo.SetValue(null, bundle);
                        if (attribute.swapStubbedShaders)
                        {
                            swapShadersBundles.Add(bundle);
                        }
                    }
                    else
                    {
                        GrooveSUPatcher.logger.LogWarning($"Load Asset Bundle attribute failed to load bundle at path {path}");
                    }
                }
            }
            if (swapShadersBundles.Count > 0)
            {
                GameObject shaderSwapper = new GameObject("GrooveSharedUtils_StubbedShaderSwapper");
                UnityEngine.Object.DontDestroyOnLoad(shaderSwapper);
                shaderSwapper.AddComponent<StubbedShaderSwapper>().BeginSwapStubbedShaders(swapShadersBundles);
            }
            settingsCache.Clear();
        }
        public class StubbedShaderSwapper : MonoBehaviour 
        {
            private Dictionary<Shader, Shader> stubbedToRealShaderCache = new Dictionary<Shader, Shader>();
            private List<AssetBundle> swapShadersBundles;
            public void BeginSwapStubbedShaders(List<AssetBundle> swapShadersBundles) 
            {
                this.swapShadersBundles = swapShadersBundles;
                StartCoroutine(nameof(SwapStubbedShaders));
            }
            public IEnumerator SwapStubbedShaders()
            {
                foreach (AssetBundle bundle in swapShadersBundles)
                {
                    yield return SwapStubbedShaders(bundle);
                }
                Destroy(base.gameObject);
            }
            public IEnumerator SwapStubbedShaders(AssetBundle bundle)
            {
                AssetBundleRequest allMatsRequest = bundle.LoadAllAssetsAsync<Material>();
                yield return new WaitUntil(() => allMatsRequest.isDone);
                int length = allMatsRequest.allAssets.Length;
                //Debug.Log($"swapping {length} stubbed shaders for bundle");
                for (int i = 0; i < length; i++)
                {
                    Material mat = (Material)allMatsRequest.allAssets[i];
                    if (!stubbedToRealShaderCache.TryGetValue(mat.shader, out Shader realShader))
                    {
                        string name = mat.shader.name;
                        if (name.StartsWith("Stubbed"))
                        {
                            AsyncOperationHandle<Shader> loadRealShaderOperation = default;

                            string path = name.Substring(7) + ".shader";
                            //Debug.Log(name);
                            //Debug.Log(path);
                            try
                            {
                                loadRealShaderOperation = Addressables.LoadAssetAsync<Shader>(path);
                            }
                            catch (Exception ex)
                            {
                                GroovyLogger.Log(LogLevel.Error, ex.ToString());
                            }
                            if (loadRealShaderOperation.IsValid())
                            {
                                //Debug.Log("Valid!");
                                yield return new WaitUntil(() => loadRealShaderOperation.IsDone);
                                realShader = loadRealShaderOperation.Result;
                            }
                        }
                        stubbedToRealShaderCache[mat.shader] = realShader;
                    }
                    mat.shader = realShader ?? mat.shader;
                    //Debug.Log("Final Shader: " + mat.shader.name);
                }
            }
        }

        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
        public sealed class SettingsAttribute : Attribute
        {
            public string defaultRelativeFolder = string.Empty;
        }
        [Obsolete(nameof(OptInAttribute) + " should be accessed from " + nameof(HG.Reflection.SearchableAttribute))]
        public new class OptInAttribute { }
    }
}
