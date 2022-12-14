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

namespace GrooveSharedUtils.Frames
{
    public class EliteFrame : EliteFrame<EliteDef, EquipmentDef, BuffDef> { }
    public class EliteFrame<TEliteDef, TEquipmentDef, TBuffDef> : BaseFrame where TEliteDef : EliteDef where TEquipmentDef : EquipmentDef where TBuffDef : BuffDef
    {
        public TEliteDef[] EliteDefs { get; private set; }
        public TEquipmentDef EliteEquipmentDef { get; private set; }
        public TBuffDef BuffDef { get; private set; }
        public string name;
        public EliteTierInfo[] tierInfos;
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

        protected override IEnumerable<object> Assets => new object[] { EliteDefs, EliteEquipmentDef, BuffDef };
        protected override internal void BuildInternal(BaseModPlugin callingMod)
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
            string tokenPrefix = callingMod.adjustedGeneratedTokensPrefix;
            string modifierToken = overrideEliteModifierToken ?? string.Format("{1}ELITE_MODIFIER_{0}", token, tokenPrefix);
            EliteDefs = new TEliteDef[tierInfos.Length];
            for(int i = 0; i < tierInfos.Length; i++)
            {
                EliteTierInfo info = tierInfos[i];
                TEliteDef eliteDef = ScriptableObject.CreateInstance<TEliteDef>();
                eliteDef.name = info.FormatEliteDefName(name).EnsurePrefix("ed");
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
