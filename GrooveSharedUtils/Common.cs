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
using static R2API.RecalculateStatsAPI;

namespace GrooveSharedUtils
{
    public static class Common
    {
        [SystemInitializer]
        public static void Init()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += Events.GlobalEventManager_OnHitEnemy;
            IL.RoR2.HealthComponent.TakeDamage += Events.HealthComponent_TakeDamage;
        }

        public static class Dependencies
        {
            public const string R2API = "com.bepis.r2api";
            public const string GrooveSharedUtils = "com.groovesalad.GrooveSharedUtils";
            public const string MoonstormSharedUtils = "com.TeamMoonstorm.MoonstormSharedUtils";
        }
        public static class Events
        {
            internal static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
            {
                orig(self, damageInfo, victim);
                if (damageInfo.procCoefficient == 0f || damageInfo.rejected)
                {
                    return;
                }
                if (!NetworkServer.active)
                {
                    return;
                }
                if (damageInfo.attacker && damageInfo.procCoefficient > 0f)
                {
                    if (onHitEnemyServer != null)
                    {
                        onHitEnemyServer.Invoke(damageInfo, victim);
                    }
                }
            }
            internal static void HealthComponent_TakeDamage(ILContext il)
            {
                ILCursor c = new ILCursor(il);

                int damageLocIndex = -1;

                bool ILFound = c.TryGotoNext(MoveType.After,
                    x => x.MatchStloc(out damageLocIndex),
                    x => x.MatchLdarg(1),
                    x => x.MatchLdcI4(7),
                    x => x.MatchStfld<DamageInfo>(nameof(DamageInfo.damageColorIndex))
                    );

                if (ILFound)
                {
                    c.MoveAfterLabels();
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldarg_1);
                    c.Emit(OpCodes.Ldloc, damageLocIndex);
                    c.EmitDelegate<Func<HealthComponent, DamageInfo, float, float>>((victim, damageInfo, damage) =>
                    {
                        if (onProcessDamageServer != null)
                        {
                            onProcessDamageServer.Invoke(victim, damageInfo, ref damage);
                        }
                        return damage;
                    });
                    c.Emit(OpCodes.Stloc, damageLocIndex);
                }
                else { GSUtil.Log(BepInEx.Logging.LogLevel.Error, "Take Damage IL hook failed!"); }
            }
            public delegate void OnHitEnemyDelegate(DamageInfo damageInfo, GameObject victim);
            public delegate void ProcessDamageDelegate(HealthComponent victim, DamageInfo damageInfo, ref float damage);
            public static event OnHitEnemyDelegate onHitEnemyServer;
            public static event ProcessDamageDelegate onProcessDamageServer;
        }
        public static class Shaders
        {
            public static Shader standard = Addressables.LoadAssetAsync<Shader>("48dca5b99d113b8d11006bab44295342").WaitForCompletion();
            public static Shader cloudRemap = Addressables.LoadAssetAsync<Shader>("bbffe49749c91724d819563daf91445d").WaitForCompletion();
            public static Shader opaqueCloudRemap = Addressables.LoadAssetAsync<Shader>("a035a371a79a19c468ec4e6dc40911c5").WaitForCompletion();
            public static Shader intersectionCloudRemap = Addressables.LoadAssetAsync<Shader>("43a6c7a9084ef9743ab45ee8d5f3c4e9").WaitForCompletion();
            public static Shader distortion = Addressables.LoadAssetAsync<Shader>("f6bd449dcf2a4496da3d2ad0c3881450").WaitForCompletion();
            public static Shader solidParallax = Addressables.LoadAssetAsync<Shader>("302e1057ea9d0e74dab5a0de5cbf611c").WaitForCompletion();
            public static Shader forwardPlanet = Addressables.LoadAssetAsync<Shader>("94b2ede73cf555f4f8549dc24b957446").WaitForCompletion();
            public static Shader distantWater = Addressables.LoadAssetAsync<Shader>("d48a4aa52cd665f45a89801d053c38de").WaitForCompletion();
            public static Shader triplanarTerrain = Addressables.LoadAssetAsync<Shader>("cd44d5076b47fbc4d8872b2a500b78f8").WaitForCompletion();
            public static Shader snowTopped = Addressables.LoadAssetAsync<Shader>("ec2c273472427df41846b25c110155c2").WaitForCompletion();
            public static Shader wavyCloth = Addressables.LoadAssetAsync<Shader>("69d9da0a01c9f774e8e80f16ecd381b0").WaitForCompletion();
        }
    }
        
}
