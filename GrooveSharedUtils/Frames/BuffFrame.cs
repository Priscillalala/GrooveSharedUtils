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
using GrooveSharedUtils.ScriptableObjects;
using JetBrains.Annotations;
using System.Collections;

namespace GrooveSharedUtils.Frames
{
    public class BuffFrame : BuffFrame<BuffDef> { }
    public class BuffFrame<IBuffDef> : BaseFrame where IBuffDef : BuffDef
    {
        public string name;
        public Color buffColor = Color.white;
        public bool canStack = false;
        public EliteDef eliteDef = null;
        public Sprite icon = null;
        public bool isCooldown = false;
        public bool isDebuff = false;
        public bool isHidden = false;
        public NetworkSoundEventDef startSfx = null;
        public IBuffDef BuffDef { get; private set;}
        protected override IEnumerable GetAssets()
        {
            yield return BuffDef;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            BuffDef = ScriptableObject.CreateInstance<IBuffDef>();
            GSUtil.EnsurePrefix(ref name, "bd");
            BuffDef.name = name;
            BuffDef.buffColor = buffColor;
            BuffDef.canStack = canStack;
            BuffDef.eliteDef = eliteDef;
            BuffDef.iconSprite = icon;
            BuffDef.isCooldown = isCooldown;
            BuffDef.isDebuff = isDebuff;
            BuffDef.isHidden = isHidden;
            BuffDef.startSfx = startSfx;
        }
    }
}
