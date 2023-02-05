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
using RoR2.Skills;
using BepInEx.Configuration;
using GrooveSharedUtils.ScriptableObjects;
using HG;
using System.Linq;
using BepInEx.Logging;

namespace GrooveSharedUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AssetDisplayCaseAttribute : HG.Reflection.SearchableAttribute
    {
        internal static Dictionary<Assembly, AssetFieldLocator> assemblyToAssetFieldLocator = new Dictionary<Assembly, AssetFieldLocator>();
        internal static (Type, string)[] _typeToCommonPrefix = new (Type, string)[]
        {
            (typeof(NetworkSoundEventDef), "nse"),
            (typeof(BuffDef), "bd"),
            (typeof(EliteDef), "ed"),
            (typeof(DirectorCardCategorySelection), "dccs"),
            (typeof(DccsPool), "dp"),
            (typeof(ItemDisplayRuleSet), "idrs"),
        };

        public static ReadOnlyArray<(Type, string)> typeToCommonPrefix = new ReadOnlyArray<(Type, string)>(_typeToCommonPrefix);
        internal static Dictionary<Type, List<string>> typeToPossiblePrefixesCache = new Dictionary<Type, List<string>>();
        public static bool TryDisplayAsset(object asset, Assembly assembly) => TryDisplayAsset(asset, GetAssetName(asset), assembly);
        public static bool TryDisplayAsset(object asset, string assetName, Assembly assembly)
        {
            if (asset == null || string.IsNullOrEmpty(assetName)) 
            {
                return false;
            }
            if (!assemblyToAssetFieldLocator.TryGetValue(assembly, out AssetFieldLocator assetFieldLocator)) 
            {
                return false;
            }
            Type assetType = asset.GetType();
            bool result = false;
            if (assetFieldLocator.TryGetValue((assetName, assetType), out FieldInfo field))
            {
                field.SetValue(null, asset);
                result = true;
            }
            List<string> commonPrefixes = typeToPossiblePrefixesCache.GetOrCreateValue(assetType, () => 
            (from (Type, string) pair in typeToCommonPrefix 
             where pair.Item1.IsAssignableFrom(assetType) 
             select pair.Item2).ToList());
            foreach (string commonPrefix in commonPrefixes)
            {
                if (assetName.StartsWith(commonPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (assetFieldLocator.TryGetValue((assetName.Substring(commonPrefix.Length), assetType), out FieldInfo fieldFromPrefix))
                    {
                        fieldFromPrefix.SetValue(null, asset);
                        result = true;
                    }
                }
            }
            return result;
        }
        public static string GetAssetName(object asset)
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
            if (asset is SkillDef sd && !string.IsNullOrEmpty(sd.skillName))
            {
                return sd.skillName;
            }
            if (asset is ModdedScriptableObject moddedScriptableObject)
            {
                return moddedScriptableObject.cachedName;
            }
            if (asset is ArtifactDef artifactDef)
            {
                return artifactDef.cachedName;
            }
            if (asset is UnlockableDef unlockableDef)
            {
                return unlockableDef.cachedName;
            }
            if (asset is AchievementDef achievement)
            {
                return achievement.identifier;
            }
            if (asset is ConfigFile configFile)
            {
                return System.IO.Path.GetFileNameWithoutExtension(configFile.ConfigFilePath);
            }
            if (asset is ConfigEntryBase configEntry && configEntry.Definition != null)
            {
                return configEntry.Definition.Key;
            }
            if (asset is UnityEngine.Object obj)
            {
                return obj.name;
            }
            return null;
        }

        internal static void PatcherInit()
        {
            List<AssetDisplayCaseAttribute> assetDisplayCaseAttributes = new List<AssetDisplayCaseAttribute>();
            GetInstances(assetDisplayCaseAttributes);

            foreach (AssetDisplayCaseAttribute attribute in assetDisplayCaseAttributes)
            {
                if (attribute.target is Type type)
                {
                    FindAssetFields(type, assemblyToAssetFieldLocator.GetOrCreateValue(type.Assembly));
                }
                else
                {
                    GrooveSUPatcher.logger.LogWarning($"Asset Display case attribute targets {attribute.target.GetType()} which should be a class instead!");
                }
            }
        }
        private static void FindAssetFields(Type type, AssetFieldLocator assetFieldLocator)
        {
            GrooveSUPatcher.logger.LogInfo("display case: " + type);
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                GrooveSUPatcher.logger.LogInfo("display case field: " + fieldInfo.Name);
                assetFieldLocator.Add((fieldInfo.Name, fieldInfo.FieldType), fieldInfo);
            }
            foreach (Type nestedType in type.GetNestedTypes())
            {
                FindAssetFields(nestedType, assetFieldLocator);
            }
        }

        [Obsolete(nameof(OptInAttribute) + " should be accessed from " + nameof(HG.Reflection.SearchableAttribute))]
        public new class OptInAttribute { }
    }
}
