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

namespace GrooveSharedUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigurableAttribute : HG.Reflection.SearchableAttribute
    {
        public static HashSet<Type> configDisabledModuleTypes = new HashSet<Type>();
        static ConfigurableAttribute()
        {
            bind = typeof(ConfigFile).GetMethods(BindingFlags.Instance | BindingFlags.Public).Where((MethodInfo info) => info.Name == nameof(ConfigFile.Bind)).FirstOrDefault();
        }
        public static MethodInfo bind;
        public static Dictionary<Type, (MethodInfo genericBind, PropertyInfo genericBoxedValue)> genericsCache = new Dictionary<Type, (MethodInfo genericBind, PropertyInfo genericBoxedValue)>();

        public string configName = null;
        public object defaultValue = null;
        public string section = null;
        public string name = null;
        public string description = null;
        public string resetForVersion = null;
        public bool targetsType => target is Type;
        public bool targetsField => target is FieldInfo;

        public void BindToField(FieldInfo fieldInfo, ConfigFile configFile, SettingsAttribute settings, Assembly assembly)
        {
            Type fieldType = fieldInfo.FieldType;
            (MethodInfo genericBind, PropertyInfo genericBoxedValue) = genericsCache.GetOrCreateValue(fieldType, () => (bind.MakeGenericMethod(fieldType), typeof(ConfigEntry<>).MakeGenericType(fieldType).GetProperty("BoxedValue")));
            defaultValue = defaultValue != null && fieldType.IsAssignableFrom(defaultValue.GetType()) ? defaultValue : fieldInfo.GetValue(null);
            string fieldName = GSUtil.InternalToExternalName(fieldInfo.Name);
            Type declaringType = fieldInfo.DeclaringType;
            string declaringTypeName = GSUtil.InternalToExternalName(declaringType.Name);

            switch (settings.configStructure)
            {
                case ConfigStructure.Normal:
                    section = section ?? declaringTypeName;
                    name = name ?? fieldName;
                    break;
                case ConfigStructure.Flattened:
                    section = section ?? (settings.trimConfigNamespaces ? declaringType.Namespace.Split('.').LastOrDefault() : declaringType.Namespace);
                    name = name ?? declaringTypeName + ": " + fieldName;
                    break;
            }
            ConfigEntryBase configEntry = (ConfigEntryBase)genericBind.Invoke(configFile, new object[] { new ConfigDefinition(section, name), defaultValue, description != null ? new ConfigDescription(description) : null });
            OnBound(configEntry, assembly);
            fieldInfo.SetValue(null, configEntry.BoxedValue);
        }
        public void BindToType(Type t, ConfigFile configFile, SettingsAttribute settings, Assembly assembly)
        {
            if (!(defaultValue is bool))
            {
                defaultValue = true;
            }
            string typeName = GSUtil.InternalToExternalName(t.Name);
            switch (settings.configStructure)
            {
                case ConfigStructure.Normal:
                    section = section ?? typeName;
                    name = name ?? "Enable " + typeName;
                    break;
                case ConfigStructure.Flattened:
                    section = section ?? (settings.trimConfigNamespaces ? t.Namespace.Split('.').LastOrDefault() : t.Namespace);
                    name = name ?? typeName;
                    description = description ?? "Enable " + typeName + "?";
                    break;
            }
            ConfigEntry<bool> configEntry = configFile.Bind(new ConfigDefinition(section, name), (bool)defaultValue, description != null ? new ConfigDescription(description) : null);
            OnBound(configEntry, assembly);
            if (!configEntry.Value)
            {
                configDisabledModuleTypes.Add(t);
            }
        }
        public void OnBound(ConfigEntryBase configEntry, Assembly assembly) 
        {
            AssetDisplayCaseAttribute.TryDisplayAsset(configEntry, assembly);
            if (!string.IsNullOrEmpty(resetForVersion) && ConfigManager.TryGetPreviousConfigVersion(configEntry.ConfigFile, out Version prevVersion) && configEntry.ConfigFile._ownerMetadata != null)
            {
                Version resetVersion = new Version(resetForVersion);
                Version currentVersion = configEntry.ConfigFile._ownerMetadata.Version;
                Debug.Log("prev version " + prevVersion);
                Debug.Log("reset version " + resetVersion);
                Debug.Log("cur version " + currentVersion);
                if (prevVersion < resetVersion && currentVersion >= resetVersion)
                {
                    configEntry.BoxedValue = configEntry.DefaultValue;
                    Debug.Log("reset!");
                }
                else
                {
                    Debug.Log("didnt reset!");
                }
            }
        }

        public static Dictionary<Assembly, List<ConfigurableAttribute>> attributesByAssembly = new Dictionary<Assembly, List<ConfigurableAttribute>>();
        internal static void PatcherInit()
        {
            List<ConfigurableAttribute> configurableAttributes = new List<ConfigurableAttribute>();
            GetInstances(configurableAttributes);

            foreach (ConfigurableAttribute attribute in configurableAttributes)
            {
                if (attribute.targetsType || attribute.targetsField)
                {
                    Type typeTarget = attribute.target as Type;
                    FieldInfo fieldTarget = attribute.target as FieldInfo;
                    Assembly assembly = (typeTarget ?? fieldTarget.DeclaringType).Assembly;
                    attributesByAssembly.GetOrCreateValue(assembly).Add(attribute);
                }
            }

            GrooveSUPatcher.onBeforePluginInstantiated += OnBeforePluginInstantiated;
        }

        internal static void OnBeforePluginInstantiated(PluginInfo pluginInfo, Assembly assembly)
        {
            if (attributesByAssembly.TryFreeValue(assembly, out List<ConfigurableAttribute> configurableAttributes))
            {
                SettingsAttribute settings = assembly.GetCustomAttribute<SettingsAttribute>() ?? new SettingsAttribute();
                foreach (ConfigurableAttribute attribute in configurableAttributes)
                {
                    Type typeTarget = attribute.target as Type;
                    FieldInfo fieldTarget = attribute.target as FieldInfo;
                    string configName = attribute.configName ?? settings.defaultConfigName ?? pluginInfo.Metadata.GUID;
                    if (attribute.targetsType)
                    {
                        if (!typeTarget.IsSubclassOf(typeof(ModModule)))
                        {
                            GrooveSUPatcher.logger.LogWarning($"Configurable attribute targets type {typeTarget.Name} which does not inherit from {nameof(ModModule)}!");
                            continue;
                        }
                        attribute.BindToType(typeTarget, ConfigManager.GetOrCreate(configName, pluginInfo.Metadata), settings, assembly);
                    }
                    else if (attribute.targetsField)
                    {
                        if (!fieldTarget.IsStatic)
                        {
                            GrooveSUPatcher.logger.LogWarning($"Configurable attribute targets field {fieldTarget.Name} which MUST be static!");
                            continue;
                        }
                        attribute.BindToField(fieldTarget, ConfigManager.GetOrCreate(configName, pluginInfo.Metadata), settings, assembly);
                    }
                }
            }
        }

        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
        public sealed class SettingsAttribute : Attribute
        {
            public string defaultConfigName = null;
            public ConfigStructure configStructure = ConfigStructure.Normal;
            public bool trimConfigNamespaces = false;
        }
        [Obsolete(nameof(OptInAttribute) + " should be accessed from " + nameof(HG.Reflection.SearchableAttribute))]
        public new class OptInAttribute { }
    }
}
//public object Value { get => _value; private set => _value = value; }
//private object _value;
//public object ConfigEntry { get => _configEntry; private set => _configEntry = value; }
//private object _configEntry;

/*public void Bind()
{
    Type type = target as Type;
    FieldInfo fieldInfo = null;
    bool targetsType = type != null;
    bool targetsFieldInfo = fieldInfo != null;
    if(!targetsType && !targetsFieldInfo)
    {
        return;
    }
    BaseModPlugin plugin = (type ?? fieldInfo.DeclaringType).Assembly.GrooveSharedUtilsInfo().plugin;
    if (!plugin)
    {
        return;
    }
    ModEnvironment env = plugin.Environment;
    ConfigFile configFile = Util.GetOrCreateConfig(name ?? env.defaultConfigName);
    if (configFile == null || env.configStructure < 0 || env.configStructure >= ModEnvironment.ConfigStructure.Count)
    {
        return;
    }
    if(targetsType)
    {
        BindToType(type, configFile, env);
    }
    else if(targetsFieldInfo)
    {
        BindToField(fieldInfo, configFile, plugin.Environment);
    }
}*/
