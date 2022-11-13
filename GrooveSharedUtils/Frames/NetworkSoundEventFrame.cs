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
    public class NetworkSoundEventFrame<TSkillDef> : BaseFrame where TSkillDef : NetworkSoundEventDef
    {
        public string name;
        public string eventName;
        public TSkillDef NetworkSoundEventDef { get; private set; }
        protected override IEnumerable<object> Assets => new object[] { NetworkSoundEventDef };
        protected internal override void BuildInternal(BaseModPlugin callingMod)
        {
            NetworkSoundEventDef = ScriptableObject.CreateInstance<TSkillDef>();
            NetworkSoundEventDef.name = name.EnsurePrefix("nse");
            NetworkSoundEventDef.eventName = eventName;
        }
    }
}
