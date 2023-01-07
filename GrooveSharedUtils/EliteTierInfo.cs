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
using R2API.ScriptableObjects;
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using System.Runtime.CompilerServices;
using System.Collections;
using RoR2.ContentManagement;
using UnityEngine.Events;
using BepInEx.Configuration;

namespace GrooveSharedUtils
{
    public struct EliteTierInfo
    {
        public EliteTier tier;
        public float healthBoostCoefficient;
        public float damageBoostCoefficient;
        public string eliteDefNameFormat;
        public EliteTierInfo(EliteTier tier, float healthBoostCoefficient, float damageBoostCoefficient, string eliteDefNameFormat = null)
        {
            this.tier = tier;
            this.healthBoostCoefficient = healthBoostCoefficient;
            this.damageBoostCoefficient = damageBoostCoefficient;
            this.eliteDefNameFormat = eliteDefNameFormat;
        }
        public string FormatEliteDefName(string name)
        {
            if (string.IsNullOrEmpty(eliteDefNameFormat))
            {
                return name;
            }
            return string.Format(eliteDefNameFormat, name);
        }
        public static EliteTierInfo tierOneDefault = new EliteTierInfo(EliteTier.TierOne, 4f, 2f);
        public static EliteTierInfo honorDefault = new EliteTierInfo(EliteTier.Honor, 2.5f, 1.5f, "{0}Honor");
        public static EliteTierInfo tierTwoDefault = new EliteTierInfo(EliteTier.TierTwo, 18f, 6f);
        public static EliteTierInfo lunarEliteDefault = new EliteTierInfo(EliteTier.Lunar, 2f, 2f);
    }
}
