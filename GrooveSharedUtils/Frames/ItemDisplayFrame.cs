/*using BepInEx;
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
using GrooveSharedUtils.ScriptableObjects;

namespace GrooveSharedUtils.Frames
{
    public class ItemDisplayFrame : BaseFrame
    {
        public UnityEngine.Object keyAsset;
        public ItemDisplayRuleType ruleType = ItemDisplayRuleType.ParentedPrefab;
        public LimbFlags limbMask = LimbFlags.None;
        public GameObject displayPrefab;
        public string[] idrValuesToParse = Array.Empty<string>();
        public ItemDisplayRuleDict ItemDisplayRuleDict { get; private set; }
        protected override IEnumerable<object> Assets => new object[] { ItemDisplayRuleDict };
        protected internal override void BuildInternal(BaseModPlugin callingMod)
        {
            ItemDisplayRuleDict = new ItemDisplayRuleDict();
        }
    }
}*/
