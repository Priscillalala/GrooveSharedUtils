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
using R2API.ScriptableObjects;
using R2API;
using UnityEngine.AddressableAssets;
using HG;
using RoR2.ExpansionManagement;
using System.Linq;
using JetBrains.Annotations;
using System.Collections;

namespace GrooveSharedUtils.Frames
{
    public class EliteFrame : EliteFrame<EliteFrame, EliteDef, EquipmentDef, BuffDef> { }
    public class EliteFrame<TEliteDef, TEquipmentDef, TBuffDef> : EliteFrame<EliteFrame<TEliteDef, TEquipmentDef, TBuffDef>, TEliteDef, TEquipmentDef, TBuffDef>
        where TEliteDef : EliteDef
        where TEquipmentDef : EquipmentDef
        where TBuffDef : BuffDef { }
    public abstract class EliteFrame<TFrame, TEliteDef, TEquipmentDef, TBuffDef> : Frame<TFrame> 
        where TFrame : EliteFrame<TFrame, TEliteDef, TEquipmentDef, TBuffDef>
        where TEliteDef : EliteDef 
        where TEquipmentDef : EquipmentDef 
        where TBuffDef : BuffDef
    {
        public string name;
        public EliteTierInfo[] eliteTiers;
        public Color eliteColor = Color.white;
        public string overrideEliteModifierToken = null;
        public Texture2D eliteRamp;
        public Color? overrideBuffColor = null;
        public Sprite buffIcon = null;
        public NetworkSoundEventDef buffStartSfx = null;
        public string overrideEquipmentNameToken = null;
        public string overrideEquipmentPickupToken = null;
        public string overrideEquipmentDescriptionToken = null;
        public string overrideEquipmentLoreToken = null;
        public Sprite equipmentIcon = null;
        public GameObject equipmentModelPrefab = null;
        public float? overrideEquipmentDropOnDeathChance = null;
        public ExpansionDef requiredExpansion = null;
        public TEliteDef[] EliteDefs { get; private set; }
        public TEquipmentDef EliteEquipmentDef { get; private set; }
        public TBuffDef BuffDef { get; private set; }
        public TFrame SetEliteTiers(params EliteTierInfo[] eliteTiers)
        {
            this.eliteTiers = eliteTiers;
            return this as TFrame;
        }
        protected override IEnumerable GetAssets()
        {
            yield return EliteDefs;
            yield return EliteEquipmentDef;
            yield return BuffDef;
        }
        protected override internal void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            BuffFrame<TBuffDef> buffFrame = new BuffFrame<TBuffDef>
            {
                name = $"Elite{name}",
                buffColor = overrideBuffColor ?? eliteColor,
                canStack = false,
                icon = buffIcon,
                isCooldown = false,
                isDebuff = false,
                isHidden = false,
                startSfx = buffStartSfx,
            };
            buffFrame.BuildInternal(callingMod);
            BuffDef = buffFrame.BuffDef;

            EquipmentFrame<TEquipmentDef> equipmentFrame = new EquipmentFrame<TEquipmentDef>
            {
                name = $"Elite{name}Equiment",
                overrideNameToken = overrideEquipmentNameToken,
                overridePickupToken = overrideEquipmentPickupToken,
                overrideDescriptionToken = overrideEquipmentDescriptionToken,
                overrideLoreToken = overrideEquipmentLoreToken,
                icon = equipmentIcon,
                pickupModelPrefab = equipmentModelPrefab,
                dropOnDeathChance = overrideEquipmentDropOnDeathChance ?? 0.00025f,
                requiredExpansion = requiredExpansion,
                canBeRandomlyTriggered = false,
                canDrop = false,
                cooldown = 25f,
                passiveBuffDef = BuffDef,
            };
            equipmentFrame.BuildInternal(callingMod);
            EliteEquipmentDef = equipmentFrame.EquipmentDef;
            

            string token = name.ToUpperInvariant();
            string tokenPrefix = callingMod ? callingMod.adjustedGeneratedTokensPrefix : string.Empty;
            string modifierToken = overrideEliteModifierToken ?? string.Format("{1}ELITE_MODIFIER_{0}", token, tokenPrefix);
            EliteDefs = new TEliteDef[eliteTiers.Length];
            for(int i = 0; i < eliteTiers.Length; i++)
            {
                EliteTierInfo info = eliteTiers[i];
                TEliteDef eliteDef = ScriptableObject.CreateInstance<TEliteDef>();
                string eliteName = info.FormatEliteDefName(name);
                GSUtil.EnsurePrefix(ref eliteName, "ed");
                eliteDef.name = eliteName;
                eliteDef.eliteEquipmentDef = EliteEquipmentDef;
                eliteDef.color = eliteColor;
                eliteDef.modifierToken = modifierToken;
                eliteDef.shaderEliteRampIndex = 0;
                eliteDef.healthBoostCoefficient = info.healthBoostCoefficient;
                eliteDef.damageBoostCoefficient = info.damageBoostCoefficient;

                EliteTierManager.TryAdd(eliteDef, info.tier);

                EliteDefs[i] = eliteDef;
            }
            TEliteDef firstEliteDef = EliteDefs.FirstOrDefault();
            BuffDef.eliteDef = firstEliteDef;

            if (eliteRamp != null)
            {
                EliteRamp.AddRamp(firstEliteDef, eliteRamp);
            }
        }
    }
}
