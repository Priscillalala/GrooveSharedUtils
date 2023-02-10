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
using R2API;
using System.Runtime.CompilerServices;
using System.Collections;
using GrooveSharedUtils.Frames;
using GrooveSharedUtils;
using System.Linq;
using JetBrains.Annotations;
using RoR2.ExpansionManagement;
using System.Diagnostics;

/*public static partial class _GSExtensions
{
    public static TFrame Build<TFrame>(this TFrame frame) where TFrame : Frame
    {
        frame.BuildInternal(AssemblyInfo.Get(Assembly.GetCallingAssembly())?.plugin);
        return frame;
    }
}*/

namespace GrooveSharedUtils.Frames
{
    public abstract class Frame<TFrame> : Frame
        where TFrame : Frame<TFrame>
    {
        /*public TFrame Build()
        {
            BuildForAssembly(Assembly.GetCallingAssembly());
            return this as TFrame;
        }*/
        [Obsolete(nameof(SettingsAttribute) + " should be accessed from " + nameof(Frame))]
        public new class SettingsAttribute { }
        [Obsolete(nameof(DefaultExpansionDefAttribute) + " should be accessed from " + nameof(Frame))]
        public new class DefaultExpansionDefAttribute { }
    }
    public abstract class Frame : IEnumerator
    {
        [Obsolete("Current should not be used.")]
        public object Current => iterator.Current;
        protected IEnumerator iterator;
        protected SettingsAttribute settings = defaultSettings;
        protected ExpansionDef defaultExpansionDef = null;
        public Frame()
        {
            GroovyLogger.Log(BepInEx.Logging.LogLevel.Info, "Frame created: " + this.GetType());
            iterator = BuildIterator();
            StackTrace trace = new StackTrace();
            for (int i = 0; i < trace.FrameCount; i++)
            {
                
                Assembly assembly = trace.GetFrame(i)?.GetMethod()?.DeclaringType?.Assembly;
                if (assembly != null && assembly != typeof(Frame).Assembly) 
                {
                    GroovyLogger.Log(BepInEx.Logging.LogLevel.Info, $"Found Owner: {assembly.GetName().Name}");
                    SetOwner(assembly);
                    break;
                }                
            }
        }
        protected void SetOwner(Assembly newOwner)
        {
            settings = newOwner.GetCustomAttribute<SettingsAttribute>() ?? defaultSettings;
            defaultExpansionDef = assemblyToGetDefaultExpansionDef.TryGetValue(newOwner, out Func<ExpansionDef> getExpansionDef) ? getExpansionDef() : null;
        }
        //protected abstract IEnumerable GetAssets();

        /*public IEnumerator GetEnumerator()
        {
            IEnumerable assets = GetAssets();
            if (assets.Cast<object>().Any(obj => obj == null))
            {
                GSUtil.Log(BepInEx.Logging.LogLevel.Warning, $"One or more assets from {this.GetType().Name} are null. Did you forget to Build?");
            }
            return assets.GetEnumerator();
        }*/
        [Obsolete("MoveNext should not be used. Refer to Build instead.")]
        public bool MoveNext()
        {
            return iterator.MoveNext();
        }
        [Obsolete("Reset should not be used. Construct a new Frame instead.")]
        public void Reset()
        {
            iterator.Reset();
        }
        public void Build()
        {
            while (iterator.MoveNext());
        }
        protected abstract IEnumerator BuildIterator();
        //protected internal abstract void BuildForAssembly(Assembly assembly);
        
        /*protected static string GetGeneratedTokensPrefix(Assembly assembly)
        {
            SettingsAttribute settings = assembly.GetCustomAttribute<SettingsAttribute>() ?? defaultSettings;
            return string.IsNullOrEmpty(settings.generatedTokensPrefix) ? string.Empty : settings.generatedTokensPrefix;
        }
        protected static ExpansionDef GetDefaultExpansionDef(Assembly assembly) 
        {
            if (assemblyToGetDefaultExpansionDef.TryGetValue(assembly, out Func<ExpansionDef> getExpansionDef))
            {
                return getExpansionDef();
            }
            return null;
        }*/

        internal static SettingsAttribute defaultSettings = new SettingsAttribute();
        internal static Dictionary<Assembly, Func<ExpansionDef>> assemblyToGetDefaultExpansionDef = new Dictionary<Assembly, Func<ExpansionDef>>();

        internal static void PatcherInit()
        {
            List<DefaultExpansionDefAttribute> defaultExpansionDefAttributes = new List<DefaultExpansionDefAttribute>();
            HG.Reflection.SearchableAttribute.GetInstances(defaultExpansionDefAttributes);

            foreach (DefaultExpansionDefAttribute attribute in defaultExpansionDefAttributes)
            {
                if (attribute.target is FieldInfo fieldInfo) 
                {
                    if (!fieldInfo.IsStatic)
                    {
                        UnityEngine.Debug.LogWarning($"Frame Default Expansion Def attribute targets field {fieldInfo.Name} which MUST be static!");
                        continue;
                    }
                    if (!typeof(ExpansionDef).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        UnityEngine.Debug.LogWarning($"Frame Default Expansion Def attribute targets field {fieldInfo.Name} which MUST be of type {typeof(ExpansionDef).Name}!");
                        continue;
                    }
                    if (assemblyToGetDefaultExpansionDef.ContainsKey(fieldInfo.DeclaringType.Assembly))
                    {
                        UnityEngine.Debug.LogWarning($"Assembly {fieldInfo.DeclaringType.Assembly.GetName().Name} CANNOT have more than one Frame Default Expansion Def attribute!");
                        continue;
                    }
                    assemblyToGetDefaultExpansionDef[fieldInfo.DeclaringType.Assembly] = () => (ExpansionDef)fieldInfo.GetValue(null);
                }
                else if (attribute.target is PropertyInfo propertyInfo)
                {
                    if (propertyInfo.GetMethod == null)
                    {
                        UnityEngine.Debug.LogWarning($"Frame Default Expansion Def attribute targets property {propertyInfo.Name} which MUST have a get method defined!");
                        continue;
                    }
                    if (!propertyInfo.GetMethod.IsStatic)
                    {
                        UnityEngine.Debug.LogWarning($"Frame Default Expansion Def attribute targets property {propertyInfo.Name} whose get method MUST be static!");
                        continue;
                    }
                    if (!typeof(ExpansionDef).IsAssignableFrom(propertyInfo.PropertyType))
                    {
                        UnityEngine.Debug.LogWarning($"Frame Default Expansion Def attribute targets property {propertyInfo.Name} which MUST be of type {typeof(ExpansionDef).Name}!");
                        continue;
                    }
                    if (assemblyToGetDefaultExpansionDef.ContainsKey(propertyInfo.DeclaringType.Assembly))
                    {
                        UnityEngine.Debug.LogWarning($"Assembly {propertyInfo.DeclaringType.Assembly.GetName().Name} CANNOT have more than one Frame Default Expansion Def attribute!");
                        continue;
                    }
                    assemblyToGetDefaultExpansionDef[propertyInfo.DeclaringType.Assembly] = () => (ExpansionDef)propertyInfo.GetValue(null);
                }
                else if (attribute.target is MethodInfo methodInfo)
                {
                    if (!methodInfo.IsStatic)
                    {
                        UnityEngine.Debug.LogWarning($"Frame Default Expansion Def attribute targets method {methodInfo.Name} which MUST be static!");
                        continue;
                    }
                    if (methodInfo.GetGenericArguments().Length != 0)
                    {
                        UnityEngine.Debug.LogWarning($"Frame Default Expansion Def attribute targets method {methodInfo.Name} which CANNOT have arguments!");
                        continue;
                    }
                    if (!typeof(ExpansionDef).IsAssignableFrom(methodInfo.ReturnType))
                    {
                        UnityEngine.Debug.LogWarning($"Frame Default Expansion Def attribute targets method {methodInfo.Name} which MUST return type {typeof(ExpansionDef).Name}!");
                        continue;
                    }
                    if (assemblyToGetDefaultExpansionDef.ContainsKey(methodInfo.DeclaringType.Assembly))
                    {
                        UnityEngine.Debug.LogWarning($"Assembly {methodInfo.DeclaringType.Assembly.GetName().Name} CANNOT have more than one Frame Default Expansion Def attribute!");
                        continue;
                    }
                    assemblyToGetDefaultExpansionDef[methodInfo.DeclaringType.Assembly] = () => (ExpansionDef)methodInfo.Invoke(null, Array.Empty<object>());
                }
            }
        }


        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
        public sealed class SettingsAttribute : Attribute
        {
            public string generatedTokensPrefix = string.Empty;
        }
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
        public sealed class DefaultExpansionDefAttribute : HG.Reflection.SearchableAttribute { }
    }
}
