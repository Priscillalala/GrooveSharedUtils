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
using GrooveSharedUtils.ScriptableObjects;
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using HG;
using BepInEx.Configuration;
using System.Runtime.CompilerServices;
using GrooveSharedUtils.Attributes;
using BepInEx.Logging;

namespace GrooveSharedUtils
{
    public static class ConfigManager
    {
        public static Dictionary<string, ConfigFile> configFileByName = new Dictionary<string, ConfigFile>();
        public static Dictionary<ConfigFile, Version> previousConfigFileVersion = new Dictionary<ConfigFile, Version>();
        private static readonly object ioLock = new object();
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
                string path = Path.Combine(Paths.ConfigPath, name + ".cfg");
                string fullPath = Path.GetFullPath(path);
                string previousVersionString = null;
                if (File.Exists(path))
                {
                    lock (ioLock)
                    {
                        string firstLine = File.ReadLines(path).FirstOrDefault();
                        if (!string.IsNullOrEmpty(firstLine) && firstLine.Contains("Settings file was created by plugin"))
                        {
                            int versionStart = firstLine.LastIndexOf('v') + 1;
                            if (versionStart > 0)
                            {
                                previousVersionString = firstLine.Substring(versionStart);
                            }
                        }
                    }
                }
                ConfigFile configFile = new ConfigFile(path, true, owner);
                if (!string.IsNullOrEmpty(previousVersionString))
                {
                    previousConfigFileVersion[configFile] = new Version(previousVersionString);
                }
                AssetDisplayCaseAttribute.TryDisplayAsset(configFile, assembly);
                /*if (ModPlugin.TryFind(assembly, out ModPlugin plugin))
                {
                    plugin.AddDisplayAsset(configFile);
                }*/
                return configFile;
            });

        }
        public static bool TryGetPreviousConfigVersion(ConfigFile configFile, out Version version) => previousConfigFileVersion.TryGetValue(configFile, out version);
    }
        
}
