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
    public class DamageColorFrame : DamageColorFrame<DamageColorFrame, ModdedDamageColorDef> { }
    public class DamageColorFrame<TModdedDamageColorDef> : DamageColorFrame<DamageColorFrame<TModdedDamageColorDef>, TModdedDamageColorDef> 
        where TModdedDamageColorDef : ModdedDamageColorDef { }
    public abstract class DamageColorFrame<TFrame, TModdedDamageColorDef> : Frame<TFrame> 
        where TFrame : DamageColorFrame<TFrame, TModdedDamageColorDef>, new()
        where TModdedDamageColorDef : ModdedDamageColorDef
    {
        public static TFrame Create(string name, Color color)
        {
            TFrame frame = new TFrame();
            frame.name = name;
            frame.color = color;
            return frame;
        }

        public string name;
        public Color color;
        public TModdedDamageColorDef ModdedDamageColorDef { get; private set; }
        protected override IEnumerator BuildIterator()
        {
            ModdedDamageColorDef = ScriptableObject.CreateInstance<TModdedDamageColorDef>();
            ModdedDamageColorDef.cachedName = name;
            ModdedDamageColorDef.color = color;
            yield return ModdedDamageColorDef;
        }
    }
}
