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

namespace GrooveSharedUtils.Frames
{
    public class NetworkSoundEventFrame : NetworkSoundEventFrame<NetworkSoundEventDef> { }
    public class NetworkSoundEventFrame<TNetworkSoundEventDef> : BaseFrame<NetworkSoundEventFrame<TNetworkSoundEventDef>> where TNetworkSoundEventDef : NetworkSoundEventDef
    {
        public string name;
        public string eventName;
        public TNetworkSoundEventDef NetworkSoundEventDef { get; private set; }
        internal override object[] Assets => new object[] { NetworkSoundEventDef };
        internal override void BuildInternal(BaseModPlugin callingMod)
        {
            NetworkSoundEventDef = ScriptableObject.CreateInstance<TNetworkSoundEventDef>();
            NetworkSoundEventDef.name = name.EnsurePrefix("nse");
            NetworkSoundEventDef.eventName = eventName;
        }
    }
}
