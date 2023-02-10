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
    public class NetworkSoundEventFrame : NetworkSoundEventFrame<NetworkSoundEventFrame, NetworkSoundEventDef> { }
    public class NetworkSoundEventFrame<TNetworkSoundEventDef> : NetworkSoundEventFrame<NetworkSoundEventFrame<TNetworkSoundEventDef>, TNetworkSoundEventDef>
        where TNetworkSoundEventDef : NetworkSoundEventDef { }
    public abstract class NetworkSoundEventFrame<TFrame, TNetworkSoundEventDef> : Frame<TFrame> 
        where TFrame : NetworkSoundEventFrame<TFrame, TNetworkSoundEventDef>, new()
        where TNetworkSoundEventDef : NetworkSoundEventDef
    {
        public static TFrame Create(string name, string eventName)
        {
            TFrame frame = new TFrame();
            frame.name = name;
            frame.eventName = eventName;
            return frame;
        }
        public string name;
        public string eventName;
        public TNetworkSoundEventDef NetworkSoundEventDef { get; private set; }
        protected override IEnumerator BuildIterator()
        {
            NetworkSoundEventDef = ScriptableObject.CreateInstance<TNetworkSoundEventDef>();
            GSUtil.EnsurePrefix(ref name, "nse");
            NetworkSoundEventDef.name = name;
            NetworkSoundEventDef.eventName = eventName;
            yield return NetworkSoundEventDef;
        }
    }
}
