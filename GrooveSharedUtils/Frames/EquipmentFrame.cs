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
using JetBrains.Annotations;
using System.Collections;

namespace GrooveSharedUtils.Frames
{
    public class EquipmentFrame : EquipmentFrame<EquipmentFrame, EquipmentDef> { }
    public class EquipmentFrame<TEquipmentDef> : EquipmentFrame<EquipmentFrame<TEquipmentDef>, TEquipmentDef>
        where TEquipmentDef : EquipmentDef { }
    public abstract class EquipmentFrame<TFrame, TEquipmentDef> : Frame<TFrame> 
        where TFrame : EquipmentFrame<TFrame, TEquipmentDef>
        where TEquipmentDef : EquipmentDef
    {
        public string name;
        public string overrideNameToken = null;
        public string overridePickupToken = null;
        public string overrideDescriptionToken = null;
        public string overrideLoreToken = null;
        public Sprite icon = null;
        public GameObject pickupModelPrefab = null;
        public Func<EquipmentSlot, bool> performEquipmentAction = null;
        public float cooldown = 60;
        public bool appearsInSingleplayer = true;
        public bool appearsInMultiplayer = true;
        public bool canDrop = true;
        public bool canBeRandomlyTriggered = true;
        public bool enigmaCompatible = true;
        public float dropOnDeathChance = 0;
        public BuffDef passiveBuffDef = null;
        public EquipmentType equipmentType = EquipmentType.Default;
        public ColorCatalog.ColorIndex? overrideColorIndex = null;
        public ExpansionDef requiredExpansion = null;
        public UnlockableDef unlockableDef = null;
        public TEquipmentDef EquipmentDef { get; private set; }
        protected override IEnumerator BuildIterator()
        {
            string token = name.ToUpperInvariant();
            EquipmentDef = ScriptableObject.CreateInstance<TEquipmentDef>();
            EquipmentDef.name = name;
            EquipmentDef.nameToken = overrideNameToken ?? string.Format("{1}EQUIPMENT_{0}_NAME", token, settings.generatedTokensPrefix);
            EquipmentDef.pickupToken = overridePickupToken ?? string.Format("{1}EQUIPMENT_{0}_PICKUP", token, settings.generatedTokensPrefix);
            EquipmentDef.descriptionToken = overrideDescriptionToken ?? string.Format("{1}EQUIPMENT_{0}_DESC", token, settings.generatedTokensPrefix);
            EquipmentDef.loreToken = overrideLoreToken ?? string.Format("{1}EQUIPMENT_{0}_LORE", token, settings.generatedTokensPrefix);
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
            ColorCatalog.ColorIndex colorIndex = ColorCatalog.ColorIndex.None;
            switch (equipmentType)
            {
                case EquipmentType.Default:
                    colorIndex = ColorCatalog.ColorIndex.Equipment;
                    break;
                case EquipmentType.Lunar:
                    colorIndex = ColorCatalog.ColorIndex.LunarItem;
                    EquipmentDef.isLunar = true;
                    break;
                case EquipmentType.Boss:
                    colorIndex = ColorCatalog.ColorIndex.BossItem;
                    EquipmentDef.isBoss = true;
                    break;
            }
            EquipmentDef.colorIndex = overrideColorIndex ?? colorIndex;
            EquipmentDef.requiredExpansion = requiredExpansion ?? defaultExpansionDef;
            EquipmentDef.unlockableDef = unlockableDef;
            yield return EquipmentDef;

            if (performEquipmentAction != null)
            {
                EquipmentActionCatalog.Add(EquipmentDef, performEquipmentAction);
            }
        }
    }
}
