﻿using BepInEx;
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
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using HG;
using BepInEx.Configuration;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace GrooveSharedUtils
{
    public static class ManualInitializer
    {
        internal static void PatcherInit()
        {
            On.RoR2.SystemInitializerAttribute.Execute += SystemInitializerAttribute_Execute;
            LanguageCollectionManager.Init();
        }

        internal static void SystemInitializerAttribute_Execute(On.RoR2.SystemInitializerAttribute.orig_Execute orig)
        {
            Attributes.ConditionalRegisterAchievementAttribute.Init();
            EarlyAchievementCatalog.Init();
            orig();
            Interfaces.InterfaceManager.Init();
            ScriptableObjects.ModdedCatalogColorDef.Init();
            ScriptableObjects.ModdedDamageColorDef.Init();
            ArtifactActionCatalog.Init();
            BaseBuffBodyBehavior.Init();
            BaseEquipmentMasterBehavior.Init();
            BaseItemMasterBehavior.Init();
            EliteToTierCatalog.Init();
            EquipmentActionCatalog.Init();
            OverlayCatalog.Init();

        }
    }
        
}
