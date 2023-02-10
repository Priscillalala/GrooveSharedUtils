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
using BepInEx.Logging;

namespace GrooveSharedUtils
{
    internal static class GroovyLogger
    {
        internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("GrooveSharedUtils");
        internal static void Log(LogLevel level, object data) => logger.Log(level, data);
    }
}
