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
    public static class EliteToTierCatalog
    {
        public static List<EliteDef>[] eliteDefsPerTier;
        static EliteToTierCatalog()
        {
            eliteDefsPerTier = new List<EliteDef>[(int)EliteTier.Count];
            for(int i = 0; i < eliteDefsPerTier.Length; i++)
            {
                eliteDefsPerTier[i] = new List<EliteDef>();
            }
        }
        internal static void Init()
        {
            for(int i = 0; i < eliteDefsPerTier.Length; i++)
            {
                CombatDirector.EliteTierDef eliteTierDef = null;
                switch ((EliteTier)i)
                {
                    case EliteTier.TierOne: eliteTierDef = FindEliteTierFromElite(RoR2Content.Elites.Fire); break;
                    case EliteTier.Honor: eliteTierDef = FindEliteTierFromElite(RoR2Content.Elites.FireHonor); break;
                    case EliteTier.TierTwo: eliteTierDef = FindEliteTierFromElite(RoR2Content.Elites.Poison); break;
                    case EliteTier.Lunar: eliteTierDef = FindEliteTierFromElite(RoR2Content.Elites.Lunar); break;
                }
                if (eliteTierDef != null)
                {
                    eliteTierDef.eliteTypes = ArrayUtils.Join(eliteTierDef.eliteTypes, eliteDefsPerTier[i].ToArray());
                }
            }
            eliteDefsPerTier = null;
        }
        public static CombatDirector.EliteTierDef FindEliteTierFromElite(EliteDef eliteDef)
        {
            return CombatDirector.eliteTiers.FirstOrDefault(x => Array.IndexOf(x.eliteTypes, eliteDef) >= 0);
        }
        public static void TryAdd(EliteDef eliteDef, EliteTier eliteTier)
        {
            int tier = (int)eliteTier;
            if(tier >= 0 && tier < eliteDefsPerTier.Length)
            {
                eliteDefsPerTier[tier].Add(eliteDef);
            }
        }
    }
        
}
