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
using System.Runtime.CompilerServices;
using System.Collections;
using GrooveSharedUtils.Frames;
using GrooveSharedUtils;
using System.Linq;
using JetBrains.Annotations;

/*public static partial class _GSExtensions
{
    public static TFrame Build<TFrame>(this TFrame frame) where TFrame : Frame
    {
        frame.BuildInternal(AssemblyInfo.Get(Assembly.GetCallingAssembly())?.plugin);
        return frame;
    }
}*/

namespace GrooveSharedUtils.Frames
{
    public abstract class Frame<TFrame> : IEnumerable 
        where TFrame : Frame<TFrame>
    {
        protected abstract IEnumerable GetAssets();
        public TFrame Build()
        {
            BaseModPlugin.TryFind(Assembly.GetCallingAssembly(), out BaseModPlugin plugin);
            BuildInternal(plugin);
            return this as TFrame;
        }
        public IEnumerator GetEnumerator()
        {
            IEnumerable assets = GetAssets();
            if (assets.Cast<object>().Any(obj => obj == null))
            {
                GSUtil.Log(BepInEx.Logging.LogLevel.Warning, $"One or more assets from {this.GetType().Name} are null. Did you forget to Build?");
            }
            return assets.GetEnumerator();
        }
        protected internal abstract void BuildInternal([CanBeNull] BaseModPlugin callingMod);
    }
}
