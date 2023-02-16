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
using JetBrains.Annotations;

namespace GrooveSharedUtils
{
    public static class ArtifactActionCatalog
    {
        internal static Dictionary<ArtifactDef, Action<RunArtifactManager>> artifactToEnabledAction = new Dictionary<ArtifactDef, Action<RunArtifactManager>>();
        internal static Dictionary<ArtifactDef, Action<RunArtifactManager>> artifactToDisabledAction = new Dictionary<ArtifactDef, Action<RunArtifactManager>>();
        internal static void Init()
        {
            RunArtifactManager.onArtifactEnabledGlobal += RunArtifactManager_onArtifactEnabledGlobal;
            RunArtifactManager.onArtifactDisabledGlobal += RunArtifactManager_onArtifactDisabledGlobal;
        }
        private static void RunArtifactManager_onArtifactEnabledGlobal([NotNull] RunArtifactManager runArtifactManager, [NotNull] ArtifactDef artifactDef)
        {
            if (artifactToEnabledAction.TryGetValue(artifactDef, out Action<RunArtifactManager> enabledAction))
            {
                enabledAction(runArtifactManager);
            }
        }
        private static void RunArtifactManager_onArtifactDisabledGlobal([NotNull] RunArtifactManager runArtifactManager, [NotNull] ArtifactDef artifactDef)
        {
            if (artifactToDisabledAction.TryGetValue(artifactDef, out Action<RunArtifactManager> disabledAction))
            {
                disabledAction(runArtifactManager);
            }
        }
        public static void AddEnabledAction(ArtifactDef artifactDef, Action<RunArtifactManager> enabledAction)
        {
            artifactToEnabledAction.Add(artifactDef, enabledAction);
        }
        public static void AddDisabledAction(ArtifactDef artifactDef, Action<RunArtifactManager> enabledAction)
        {
            artifactToDisabledAction.Add(artifactDef, enabledAction);
        }
    }
        
}
