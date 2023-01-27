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
    public class UnlockableFrame : UnlockableFrame<UnlockableDef> { }
    public class UnlockableFrame<TUnlockableDef> : BaseFrame where TUnlockableDef : UnlockableDef
    {
        public TUnlockableDef UnlockableDef { get; private set; }

        public string name;
        public string overrideNameToken = null;
        public GameObject displayModelPrefab;
        public bool hidden;
        public Sprite achievementIcon;
        public Func<string> getHowToUnlockString = null;
        public Func<string> getUnlockedString = null;
        protected override IEnumerable GetAssets()
        {
            yield return UnlockableDef;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            string token = name.ToUpperInvariant();
            string tokenPrefix = callingMod ? callingMod.adjustedGeneratedTokensPrefix : string.Empty;
            UnlockableDef = ScriptableObject.CreateInstance<TUnlockableDef>();
            UnlockableDef.cachedName = name;
            UnlockableDef.nameToken = overrideNameToken ?? string.Format("{1}UNLOCKABLE_{0}_NAME", token, tokenPrefix);
            UnlockableDef.displayModelPrefab = displayModelPrefab;
            UnlockableDef.hidden = hidden;
            UnlockableDef.achievementIcon = achievementIcon;
            UnlockableDef.getHowToUnlockString = getHowToUnlockString ?? UnlockableDef.getHowToUnlockString;
            UnlockableDef.getUnlockedString = getUnlockedString ?? UnlockableDef.getUnlockedString;
        }
    }
}
