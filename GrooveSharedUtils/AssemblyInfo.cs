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
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using System.Runtime.CompilerServices;
using System.Collections;
using RoR2.ContentManagement;
using UnityEngine.Events;
using System.Globalization;
using BepInEx.Configuration;

namespace GrooveSharedUtils
{
    public class AssemblyInfo
    {
        public BaseModPlugin plugin;
        public Dictionary<(string, Type), FieldInfo> assetFieldLocator = new Dictionary<(string, Type), FieldInfo>(AssetFieldLocatorComparer.comparer);
        public Dictionary<string, ConfigFile> configFiles = new Dictionary<string, ConfigFile>();
        public Queue<object> pendingDisplayAssets = new Queue<object>();
        public HashSet<Type> configDisabledModuleTypes = new HashSet<Type>();

        static readonly Dictionary<Assembly, AssemblyInfo> assemblyInfos = new Dictionary<Assembly, AssemblyInfo>();
        public static AssemblyInfo Get(Assembly assembly)
        {
            return assemblyInfos.GetOrCreateValue(assembly);
        }
    }
}
