﻿using BepInEx;
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
    public class BuffFrame : BuffFrame<BuffDef> { }
    public class BuffFrame<IBuffDef> : BaseFrame<BuffFrame<IBuffDef>> where IBuffDef : BuffDef
    {
        public string name;
        public Color buffColor = Color.white;
        public bool showStacks = false;
        public EliteDef eliteDef = null;
        public Sprite icon = null;
        public bool isCooldown = false;
        public bool isDebuff = false;
        public bool isHidden = false;
        public NetworkSoundEventDef startSfx = null;
        public IBuffDef BuffDef { get; private set;}
        internal override object[] Assets => new object[] { BuffDef };
        internal override void BuildInternal(BaseModPlugin callingMod)
        {
            BuffDef = ScriptableObject.CreateInstance<IBuffDef>();

            BuffDef.name = name.EnsurePrefix("bd");
            BuffDef.buffColor = buffColor;
            BuffDef.canStack = showStacks;
            BuffDef.eliteDef = eliteDef;
            BuffDef.iconSprite = icon;
            BuffDef.isCooldown = isCooldown;
            BuffDef.isDebuff = isDebuff;
            BuffDef.isHidden = isHidden;
            BuffDef.startSfx = startSfx;
        }
    }
}
