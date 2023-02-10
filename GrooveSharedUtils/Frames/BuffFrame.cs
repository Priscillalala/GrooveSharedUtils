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
    public class BuffFrame : BuffFrame<BuffFrame, BuffDef> { }
    public class BuffFrame<TBuffDef> : BuffFrame<BuffFrame<TBuffDef>, TBuffDef> 
        where TBuffDef : BuffDef { }
    public abstract class BuffFrame<TFrame, TBuffDef> : Frame<TFrame> 
        where TFrame : BuffFrame<TFrame, TBuffDef> 
        where TBuffDef : BuffDef
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
        public TBuffDef BuffDef { get; private set; }
        protected override IEnumerator BuildIterator()
        {
            BuffDef = ScriptableObject.CreateInstance<TBuffDef>();
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
            yield return BuffDef;
        }
    }
}
