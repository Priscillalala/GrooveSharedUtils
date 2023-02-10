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
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using System.Runtime.CompilerServices;
using System.Collections;
using BepInEx.Logging;

namespace GrooveSharedUtils
{

    public abstract class ModModule<T> : ModModule where T : class
    {
        public static T instance { get; private set; }
        /*public override void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            base.Awake();
        }*/
        public ModModule() : base()
        {
            if (instance == null)
            {
                instance = this as T;
            }
        }
    }
    public abstract class ModModule : MonoBehaviour
    {
        internal static ModPlugin earlyAssignmentOwner = null;
        private ModPlugin owner;
        public ModModule()
        {
            owner = earlyAssignmentOwner;
            earlyAssignmentOwner = null;
        }
        //public virtual void Awake() { }
        //public abstract void OnModInit();
        //public abstract void OnCollectContent(AssetStream sasset);
        public virtual IEnumerator LoadContent() => null;
        public void LogDebug(LogLevel level, object data)
        {
            if (owner && owner.isDebug)
            {
                owner.Logger.Log(level, data);
            }
        }
        public void LogDebug(object data)
        {
            if (owner && owner.isDebug)
            {
                owner.Logger.Log(LogLevel.Debug, data);
            }
        }
        public void Log(LogLevel level, object data) => owner?.Logger.Log(level, data);
        public void Log(object data) => owner?.Logger.Log(LogLevel.Info, data);
    }
}
