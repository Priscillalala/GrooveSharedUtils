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
    public static class EarlyAchievementManager
    {
        public static List<AchievementDef> earlyAchievementDefs = new List<AchievementDef>();
        internal static void Init()
        {
            RoR2BepInExPack.VanillaFixes.SaferAchievementManager.OnCollectAchievementDefs += SaferAchievementManager_OnCollectAchievementDefs;
        }

        internal static void SaferAchievementManager_OnCollectAchievementDefs(List<string> identifiers, Dictionary<string, AchievementDef> identifierToAchievementDef, List<AchievementDef> achievementDefs)
        {
            foreach(AchievementDef achievementDef in earlyAchievementDefs)
            {
                if (identifierToAchievementDef.ContainsKey(achievementDef.identifier))
                {
                    GroovyLogger.Log(LogLevel.Warning, $"Class {achievementDef.type.FullName} attempted to register as achievement {achievementDef.identifier}, but class {identifierToAchievementDef[achievementDef.identifier].type.FullName} has already registered as that achievement.");
                }
                else
                {
                    identifiers.Add(achievementDef.identifier);
                    identifierToAchievementDef.Add(achievementDef.identifier, achievementDef);
                    achievementDefs.Add(achievementDef);
                }
            }
            earlyAchievementDefs = null;
        }
        public static void Add(AchievementDef earlyAchievementDef)
        {
            earlyAchievementDefs.Add(earlyAchievementDef);
        }
    }
        
}
