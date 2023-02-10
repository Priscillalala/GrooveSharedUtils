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
using RoR2.ExpansionManagement;

namespace GrooveSharedUtils
{
    public class Common
    {
        Common() { }
        /*public static partial class Dependencies
        {
            public const string R2API = "com.bepis.r2api";
            public const string GrooveSharedUtils = "com.groovesalad.GrooveSharedUtils";
            public const string MoonstormSharedUtils = "com.TeamMoonstorm.MoonstormSharedUtils";
        }*/
        public class Events
        {
            Events() { }
            static Events()
            {
                On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
                IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            }
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
                    onHitEnemyServer?.Invoke(damageInfo, victim);
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
                        onProcessDamageServer?.Invoke(victim, damageInfo, ref damage);
                        return damage;
                    });
                    c.Emit(OpCodes.Stloc, damageLocIndex);
                }
                else { GroovyLogger.Log(BepInEx.Logging.LogLevel.Error, "Take Damage IL hook failed!"); }
            }
            public delegate void OnHitEnemyDelegate(DamageInfo damageInfo, GameObject victim);
            public delegate void ProcessDamageDelegate(HealthComponent victim, DamageInfo damageInfo, ref float damage);
            public static event OnHitEnemyDelegate onHitEnemyServer;
            public static event ProcessDamageDelegate onProcessDamageServer;
        }
        public class Expansions
        {
            Expansions() { }
            public static ExpansionDef DLC1 => dlc1.WaitForCompletion();
            static LazyAddressable<ExpansionDef> dlc1 = new LazyAddressable<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset", false);
        }
        public class Idrs
        {
            Idrs() { }
            public static ItemDisplayRuleSet Commando => commando.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> commando = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Commando/idrsCommando.asset", false);
            public static ItemDisplayRuleSet Huntress => huntress.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> huntress = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Huntress/idrsHuntress.asset", false);
            public static ItemDisplayRuleSet Bandit2 => bandit2.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> bandit2 = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Bandit2/idrsBandit2.asset", false);
            public static ItemDisplayRuleSet Toolbot => toolbot.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> toolbot = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Toolbot/idrsToolbot.asset", false);
            public static ItemDisplayRuleSet Engi => engi.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> engi = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Engi/idrsEngi.asset", false);
            public static ItemDisplayRuleSet EngiTurret => engiTurret.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> engiTurret = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Engi/idrsEngiTurret.asset", false);
            public static ItemDisplayRuleSet EngiWalkerTurret => engiWalkerTurret.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> engiWalkerTurret = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Engi/idrsEngiWalkerTurret.asset", false);
            public static ItemDisplayRuleSet Mage => mage.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> mage = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Mage/idrsMage.asset", false);
            public static ItemDisplayRuleSet Merc => merc.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> merc = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Merc/idrsMerc.asset", false);
            public static ItemDisplayRuleSet Treebot => treebot.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> treebot = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Treebot/idrsTreebot.asset", false);
            public static ItemDisplayRuleSet Loader => loader.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> loader = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Loader/idrsLoader.asset", false);
            public static ItemDisplayRuleSet Croco => croco.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> croco = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Croco/idrsCroco.asset", false);
            public static ItemDisplayRuleSet Captain => captain.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> captain = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Captain/idrsCaptain.asset", false);
            public static ItemDisplayRuleSet RailGunner => railGunner.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> railGunner = new LazyAddressable<ItemDisplayRuleSet>("RoR2/DLC1/Railgunner/idrsRailGunner.asset", false);
            public static ItemDisplayRuleSet VoidSurvivor => voidSurvivor.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> voidSurvivor = new LazyAddressable<ItemDisplayRuleSet>("RoR2/DLC1/VoidSurvivor/idrsVoidSurvivor.asset", false);
            public static ItemDisplayRuleSet Scav => scav.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> scav = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Scav/idrsScav.asset", false);
            public static ItemDisplayRuleSet EquipmentDrone => equipmentDrone.WaitForCompletion();
            static LazyAddressable<ItemDisplayRuleSet> equipmentDrone = new LazyAddressable<ItemDisplayRuleSet>("RoR2/Base/Drones/idrsEquipmentDrone.asset", false);
        }
        /*public static class Shaders
        {
            public static Shader standard => Addressables.LoadAssetAsync<Shader>("48dca5b99d113b8d11006bab44295342").WaitForCompletion();
            public static Shader cloudRemap => Addressables.LoadAssetAsync<Shader>("bbffe49749c91724d819563daf91445d").WaitForCompletion();
            public static Shader opaqueCloudRemap => Addressables.LoadAssetAsync<Shader>("a035a371a79a19c468ec4e6dc40911c5").WaitForCompletion();
            public static Shader intersectionCloudRemap => Addressables.LoadAssetAsync<Shader>("43a6c7a9084ef9743ab45ee8d5f3c4e9").WaitForCompletion();
            public static Shader distortion => Addressables.LoadAssetAsync<Shader>("f6bd449dcf2a4496da3d2ad0c3881450").WaitForCompletion();
            public static Shader solidParallax => Addressables.LoadAssetAsync<Shader>("302e1057ea9d0e74dab5a0de5cbf611c").WaitForCompletion();
            public static Shader forwardPlanet => Addressables.LoadAssetAsync<Shader>("94b2ede73cf555f4f8549dc24b957446").WaitForCompletion();
            public static Shader distantWater => Addressables.LoadAssetAsync<Shader>("d48a4aa52cd665f45a89801d053c38de").WaitForCompletion();
            public static Shader triplanarTerrain => Addressables.LoadAssetAsync<Shader>("cd44d5076b47fbc4d8872b2a500b78f8").WaitForCompletion();
            public static Shader snowTopped => Addressables.LoadAssetAsync<Shader>("ec2c273472427df41846b25c110155c2").WaitForCompletion();
            public static Shader wavyCloth => Addressables.LoadAssetAsync<Shader>("69d9da0a01c9f774e8e80f16ecd381b0").WaitForCompletion();
        }*/
    }
        
}
