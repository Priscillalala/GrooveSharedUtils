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
    public class NetworkSoundEventFrame : NetworkSoundEventFrame<NetworkSoundEventDef> { }
    public class NetworkSoundEventFrame<TNetworkSoundEventDef> : BaseFrame where TNetworkSoundEventDef : NetworkSoundEventDef
    {
        public string name;
        public string eventName;
        public TNetworkSoundEventDef NetworkSoundEventDef { get; private set; }
        protected override IEnumerable GetAssets()
        {
            yield return NetworkSoundEventDef;
        }
        protected internal override void BuildInternal([CanBeNull] BaseModPlugin callingMod)
        {
            NetworkSoundEventDef = ScriptableObject.CreateInstance<TNetworkSoundEventDef>();
            GSUtil.EnsurePrefix(ref name, "nse");
            NetworkSoundEventDef.name = name;
            NetworkSoundEventDef.eventName = eventName;
        }
    }
}
