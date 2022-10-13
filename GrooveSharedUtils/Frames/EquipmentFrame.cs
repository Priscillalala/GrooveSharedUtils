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

namespace GrooveSharedUtils.Frames
{
    public class EquipmentFrame : EquipmentFrame<EquipmentDef> { }
    public class EquipmentFrame<TEquipmentDef> : BaseFrame<EquipmentFrame<TEquipmentDef>> where TEquipmentDef : EquipmentDef
    {

        public TEquipmentDef EquipmentDef { get; private set; }
        public string name;
        public string overrideNameToken = null;
        public string overridePickupToken = null;
        public string overrideDescriptionToken = null;
        public string overrideLoreToken = null;
        public Sprite icon = null;
        public GameObject pickupModelPrefab = null;
        public EquipmentActionCatalog.PerformEquipmentActionDelegate performEquipmentAction = null;
        public float cooldown = 60;
        public bool appearsInSingleplayer = true;
        public bool appearsInMultiplayer = true;
        public bool canDrop = true;
        public bool canBeRandomlyTriggered = true;
        public bool enigmaCompatible = true;
        public float dropOnDeathChance = 0;
        public BuffDef passiveBuffDef = null;
        public EquipmentType equipmentType = EquipmentType.Default;
        public ExpansionDef requiredExpansion = null;
        public UnlockableDef unlockableDef = null;

        internal override object[] Assets => new object[] { EquipmentDef };

        internal override void BuildInternal(BaseModPlugin callingMod)
        {
            string token = name.ToUpperInvariant();
            string tokenPrefix = callingMod.adjustedGeneratedTokensPrefix;
            EquipmentDef = ScriptableObject.CreateInstance<TEquipmentDef>();
            EquipmentDef.name = name;
            EquipmentDef.nameToken = overrideNameToken ?? string.Format("{1}EQUIPMENT_{0}_NAME", token, tokenPrefix);
            EquipmentDef.pickupToken = overridePickupToken ?? string.Format("{1}EQUIPMENT_{0}_PICKUP", token, tokenPrefix);
            EquipmentDef.descriptionToken = overrideDescriptionToken ?? string.Format("{1}EQUIPMENT_{0}_DESC", token, tokenPrefix);
            EquipmentDef.loreToken = overrideLoreToken ?? string.Format("{1}EQUIPMENT_{0}_LORE", token, tokenPrefix);
            EquipmentDef.pickupIconSprite = icon;
            EquipmentDef.pickupModelPrefab = pickupModelPrefab;
            EquipmentDef.cooldown = cooldown;
            EquipmentDef.appearsInMultiPlayer = appearsInMultiplayer;
            EquipmentDef.appearsInSinglePlayer = appearsInSingleplayer;
            EquipmentDef.canBeRandomlyTriggered = canBeRandomlyTriggered;
            EquipmentDef.canDrop = canDrop;
            EquipmentDef.dropOnDeathChance = dropOnDeathChance;
            EquipmentDef.enigmaCompatible = enigmaCompatible;
            EquipmentDef.passiveBuffDef = passiveBuffDef;
            switch (equipmentType)
            {
                case EquipmentType.Default:
                    EquipmentDef.colorIndex = ColorCatalog.ColorIndex.Equipment;
                    break;
                case EquipmentType.Lunar:
                    EquipmentDef.colorIndex = ColorCatalog.ColorIndex.LunarItem;
                    EquipmentDef.isLunar = true;
                    break;
                case EquipmentType.Boss:
                    EquipmentDef.colorIndex = ColorCatalog.ColorIndex.BossItem;
                    EquipmentDef.isBoss = true;
                    break;
            }
            EquipmentDef.requiredExpansion = requiredExpansion ?? callingMod.ENVIRONMENT_DefaultExpansionDef;
            EquipmentDef.unlockableDef = unlockableDef;

            if(performEquipmentAction != null)
            {
                EquipmentActionCatalog.Add(EquipmentDef, performEquipmentAction);
            }
        }
    }
}
