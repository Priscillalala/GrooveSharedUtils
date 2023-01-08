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
    public static class OverlayManager
    {
        public struct OverlayInfo
        {
            public Material material;
            public Func<CharacterModel, bool> condition;
        }
        public static List<OverlayInfo> overlayInfos = new List<OverlayInfo>();
        internal static void Init()
        {
            if(overlayInfos != null && overlayInfos.Count > 0)
            {
                IL.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
            }
            else
            {
                overlayInfos = null;
            }
        }

        private static void CharacterModel_UpdateOverlays(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            MethodReference methodReference = null;
            bool ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC1Content.Buffs).GetField(nameof(DLC1Content.Buffs.VoidSurvivorCorruptMode))),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchCallOrCallvirt(out methodReference)
                );
            if (ilfound)
            {
                foreach(OverlayInfo info in overlayInfos)
                {
                    c.Emit(OpCodes.Ldarg, 0);
                    c.EmitDelegate<Func<Material>>(() => info.material);
                    c.Emit(OpCodes.Ldarg, 0);
                    c.EmitDelegate(info.condition);
                    c.Emit(OpCodes.Call, methodReference);
                }
            }
            else
            {
                GSUtil.Log(BepInEx.Logging.LogLevel.Error, $"{nameof(OverlayManager)} IL hook failed!");
            }
        }

        public static void Add(Material overlayMaterial, Func<CharacterModel, bool> overlayCondition)
        {
            overlayInfos.Add(new OverlayInfo { material = overlayMaterial, condition = overlayCondition });
        }
    }
        
}
