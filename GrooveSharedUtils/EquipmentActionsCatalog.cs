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

namespace GrooveSharedUtils
{
    public static class EquipmentActionCatalog
    {
        public delegate bool PerformEquipmentActionDelegate(EquipmentSlot equipmentSlot);

        internal static Dictionary<EquipmentDef, PerformEquipmentActionDelegate> equipmentToAction = new Dictionary<EquipmentDef, PerformEquipmentActionDelegate>();
        internal static void Init()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;  
        }

        private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if(equipmentToAction.TryGetValue(equipmentDef, out PerformEquipmentActionDelegate performEquipmentAction))
            {
                return performEquipmentAction.Invoke(self);
            }
            return orig(self, equipmentDef);
        }

        public static void Add(EquipmentDef equipmentDef, PerformEquipmentActionDelegate performEquipmentAction)
        {
            equipmentToAction.Add(equipmentDef, performEquipmentAction);
        }
    }
        
}
