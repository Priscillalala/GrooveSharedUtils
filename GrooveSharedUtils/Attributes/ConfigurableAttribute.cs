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

        public bool targetsType => target is Type;
        public bool targetsField => target is FieldInfo;

        public object BindToField(FieldInfo fieldInfo, ConfigFile configFile, ConfigStructure structure, bool trimConfigNamespaces, out ConfigEntryBase configEntry)
        {
            Type fieldType = fieldInfo.FieldType;
            (MethodInfo genericBind, PropertyInfo genericBoxedValue) = genericsCache.GetOrCreateValue(fieldType, () => (bind.MakeGenericMethod(fieldType), typeof(ConfigEntry<>).MakeGenericType(fieldType).GetProperty("BoxedValue")));
            defaultValue = defaultValue != null && fieldType.IsAssignableFrom(defaultValue.GetType()) ? defaultValue : fieldInfo.GetValue(null);
            string fieldName = GSUtil.InternalToExternalName(fieldInfo.Name);
            Type declaringType = fieldInfo.DeclaringType;
            string declaringTypeName = GSUtil.InternalToExternalName(declaringType.Name);

            switch (structure)
            {
                case ConfigStructure.ModulesAsCategories:
                    section = section ?? declaringTypeName;
                    name = name ?? fieldName;
                    break;
                case ConfigStructure.ModulesInCategories:
                    section = section ?? (trimConfigNamespaces ? declaringType.Namespace.Split('.').LastOrDefault() : declaringType.Namespace);
                    name = name ?? declaringTypeName + ": " + fieldName;
                    break;
            }
            configEntry = (ConfigEntryBase)genericBind.Invoke(configFile, new object[] { new ConfigDefinition(section, name), defaultValue, description != null ? new ConfigDescription(description) : null });
            return configEntry.BoxedValue;
        }
        public bool BindToType(Type t, ConfigFile configFile, ConfigStructure structure, bool trimConfigNamespaces, out ConfigEntryBase configEntry)
        {
            if(!(defaultValue is bool))
            {
                defaultValue = true;
            }
            string typeName = GSUtil.InternalToExternalName(t.Name);
            switch (structure)
            {
                case ConfigStructure.ModulesAsCategories:
                    section = section ?? typeName;
                    name = name ?? "Enable " + typeName;
                    description = description ?? "Enable/Disable this content";
                    break;
                case ConfigStructure.ModulesInCategories:
                    section = section ?? (trimConfigNamespaces ? t.Namespace.Split('.').LastOrDefault() : t.Namespace);
                    name = name ?? typeName;
                    description = description ?? "Enable " + typeName + "?";
                    break;
            }
            configEntry = configFile.Bind(new ConfigDefinition(section, name), (bool)defaultValue, description != null ? new ConfigDescription(description) : null);
            return ((ConfigEntry<bool>)configEntry).Value;
        }
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
