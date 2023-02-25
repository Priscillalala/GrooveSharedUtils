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

        public ConfigEntryBase BindToField(FieldInfo fieldInfo, PluginInfo pluginInfo, SettingsAttribute settings, Assembly assembly, ConfigEntry<bool> declaringModuleConfig = null)
        {
            ConfigFile configFile = GetConfigFile(pluginInfo, settings, assembly, this);
            Type fieldType = fieldInfo.FieldType;
            (MethodInfo genericBind, PropertyInfo genericBoxedValue) = genericsCache.GetOrCreateValue(fieldType, () => (bind.MakeGenericMethod(fieldType), typeof(ConfigEntry<>).MakeGenericType(fieldType).GetProperty("BoxedValue")));
            defaultValue = defaultValue != null && fieldType.IsAssignableFrom(defaultValue.GetType()) ? defaultValue : fieldInfo.GetValue(null);
            string fieldName = GSUtil.InternalToExternalName(fieldInfo.Name);
            Type declaringType = fieldInfo.DeclaringType;
            string declaringTypeName = GSUtil.InternalToExternalName(declaringType.Name);

            string declaringModuleSection = declaringModuleConfig?.Definition.Section;
            string declaringModuleName = declaringModuleConfig?.Definition.Key;
            switch (settings.configStructure)
            {
                case ConfigStructure.Normal:
                    section = section ?? declaringModuleSection ?? declaringTypeName;
                    name = name ?? fieldName;
                    break;
                case ConfigStructure.Flattened:
                    section = section ?? declaringModuleSection ?? (settings.trimConfigNamespaces ? declaringType.Namespace.Split('.').LastOrDefault() : declaringType.Namespace);
                    name = name ?? (declaringModuleName ?? declaringTypeName) + ": " + fieldName;
                    break;
            }
            ConfigEntryBase configEntry = (ConfigEntryBase)genericBind.Invoke(configFile, new object[] { new ConfigDefinition(section, name), defaultValue, description != null ? new ConfigDescription(description) : null });
            OnBound(configEntry, assembly);
            return configEntry;
        }
        public ConfigEntry<bool> BindToModuleType(Type t, PluginInfo pluginInfo, SettingsAttribute settings, Assembly assembly)
        {
            ConfigFile configFile = GetConfigFile(pluginInfo, settings, assembly, this);
            if (!(defaultValue is bool))
            {
                defaultValue = true;
            }
            string typeName = GSUtil.InternalToExternalName(t.Name);
            switch (settings.configStructure)
            {
                case ConfigStructure.Normal:
                    section = section ?? typeName;
                    name = name ?? "Enable " + section;
                    break;
                case ConfigStructure.Flattened:
                    section = section ?? (settings.trimConfigNamespaces ? t.Namespace.Split('.').LastOrDefault() : t.Namespace);
                    name = name ?? typeName;
                    description = description ?? "Enable " + section + "?";
                    break;
            }
            ConfigEntry<bool> configEntry = configFile.Bind(new ConfigDefinition(section, name), (bool)defaultValue, description != null ? new ConfigDescription(description) : null);
            OnBound(configEntry, assembly);
            return configEntry;
        }
        public void OnBound(ConfigEntryBase configEntry, Assembly assembly) 
        {
            AssetDisplayCaseAttribute.TryDisplayAsset(configEntry, assembly);
            if (!string.IsNullOrEmpty(resetForVersion) && ConfigCatalog.TryGetPreviousConfigVersion(configEntry.ConfigFile, out Version prevVersion) && configEntry.ConfigFile._ownerMetadata != null)
            {
                Version resetVersion = new Version(resetForVersion);
                Version currentVersion = configEntry.ConfigFile._ownerMetadata.Version;
                /*Debug.Log("prev version " + prevVersion);
                Debug.Log("reset version " + resetVersion);
                Debug.Log("cur version " + currentVersion);*/
                if (prevVersion < resetVersion && currentVersion >= resetVersion)
                {
                    configEntry.BoxedValue = configEntry.DefaultValue;
                }
            }
        }
        
        internal static Dictionary<Assembly, SettingsAttribute> assemblyToSettings = new Dictionary<Assembly, SettingsAttribute>();
        internal static Dictionary<Assembly, List<ConfigurableAttribute>> attributesByAssemblyHolder = new Dictionary<Assembly, List<ConfigurableAttribute>>();
        internal static Dictionary<Type, bool> moduleToConfigEnabled = new Dictionary<Type, bool>();
        internal static Dictionary<Type, List<ConfigurableAttribute>> configurableFieldsByModuleHolder = new Dictionary<Type, List<ConfigurableAttribute>>();

        internal static readonly SettingsAttribute defaultSettings = new SettingsAttribute();
        public static bool CheckModuleTypeEnabled(Type moduleType, PluginInfo pluginInfo)
        {
            /*if (!moduleToConfigEnabled.TryGetValue(moduleType, out bool enabled))
            {
                return true;
            }*/
            return moduleToConfigEnabled.GetOrCreateValue(moduleType, () =>
            {
                SettingsAttribute settings = GetSettings(moduleType.Assembly);
                ConfigurableAttribute attribute = moduleType.GetCustomAttribute<ConfigurableAttribute>();
                ConfigEntry<bool> config = null;
                if (attribute != null)
                {
                    config = attribute.BindToModuleType(moduleType, pluginInfo, settings, moduleType.Assembly);
                }
                if (configurableFieldsByModuleHolder.TryFreeValue(moduleType, out List<ConfigurableAttribute> configurableFields))
                {
                    foreach (ConfigurableAttribute configurableField in configurableFields)
                    {
                        FieldInfo field = configurableField.target as FieldInfo;
                        field.SetValue(null, configurableField.BindToField(field, pluginInfo, settings, moduleType.Assembly, config).BoxedValue);
                    }
                }
                return config == null || config.Value;
            });
            /*if (config == null)
            {
                config = moduleType.GetCustomAttribute<ConfigurableAttribute>().BindToModuleType(moduleType, pluginInfo, settings, moduleType.Assembly);
                if (configurableFieldsByModuleHolder.TryFreeValue(moduleType, out List<ConfigurableAttribute> configurableFields))
                {
                    foreach (ConfigurableAttribute configurableField in configurableFields)
                    {
                        FieldInfo field = configurableField.target as FieldInfo;
                        field.SetValue(null, configurableField.BindToField(field, pluginInfo, settings, moduleType.Assembly, config).BoxedValue);
                    }
                }
                moduleToConfig[moduleType] = config;
            }
            return config.Value;*/
        }
        internal static SettingsAttribute GetSettings(Assembly assembly) 
        {
            return assemblyToSettings.GetOrCreateValue(assembly, () => assembly.GetCustomAttribute<SettingsAttribute>() ?? defaultSettings);
        }
        internal static ConfigFile GetConfigFile(PluginInfo pluginInfo, SettingsAttribute settings, Assembly assembly, ConfigurableAttribute attribute)
        {
            return ConfigCatalog.GetOrCreate(attribute.configName ?? settings.defaultConfigName ?? pluginInfo.Metadata.GUID, assembly, pluginInfo.Metadata);
        }
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
                    attributesByAssemblyHolder.GetOrCreateValue(assembly).Add(attribute);
                }
            }

            GrooveSUPatcher.onBeforePluginInstantiated += OnBeforePluginInstantiated;
        }
        internal static void OnBeforePluginInstantiated(PluginInfo pluginInfo, Assembly assembly)
        {
            if (attributesByAssemblyHolder.TryFreeValue(assembly, out List<ConfigurableAttribute> configurableAttributes))
            {
                SettingsAttribute settings = GetSettings(assembly);
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
                        //moduleToConfig.Add(typeTarget, null);
                        //attribute.BindToType(typeTarget, ConfigManager.GetOrCreate(configName, pluginInfo.Metadata), settings, assembly);
                    }
                    else if (attribute.targetsField)
                    {
                        if (!fieldTarget.IsStatic)
                        {
                            GrooveSUPatcher.logger.LogWarning($"Configurable attribute targets field {fieldTarget.Name} which MUST be static!");
                            continue;
                        }
                        if (typeof(ModModule).IsAssignableFrom(fieldTarget.DeclaringType))
                        {
                            configurableFieldsByModuleHolder.GetOrCreateValue(fieldTarget.DeclaringType).Add(attribute);
                        }
                        else
                        {
                            fieldTarget.SetValue(null, attribute.BindToField(fieldTarget, pluginInfo, settings, assembly).BoxedValue);
                        }
                        
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
