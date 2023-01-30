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
using GrooveSharedUtils.ScriptableObjects;
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using HG;
using BepInEx.Configuration;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;
using GrooveSharedUtils.Attributes;
using BepInEx.Logging;

namespace GrooveSharedUtils
{
    public static class ConfigManager
    {
        public static Dictionary<string, ConfigFile> configFileByName = new Dictionary<string, ConfigFile>();
        public static ConfigFile GetOrCreate(string name, BepInPlugin owner = null)
        {
            return GetOrCreate(name, Assembly.GetCallingAssembly(), owner);
        }
        public static ConfigFile GetOrCreate(string name, Assembly assembly, BepInPlugin owner = null)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return null;
            }
            return configFileByName.GetOrCreateValue(name, () =>
            {
                string path = System.IO.Path.Combine(Paths.ConfigPath, name + ".cfg");
                ConfigFile configFile = new ConfigFile(path, true, owner);
                if (BaseModPlugin.TryFind(assembly, out BaseModPlugin plugin))
                {
                    plugin.AddDisplayAsset(configFile);
                }
                return configFile;
            });

        }
    }
        
}
