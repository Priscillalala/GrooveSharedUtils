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

public static partial class _GSExtensions
{
    public static TFrame Build<TFrame>(this TFrame frame) where TFrame : BaseFrame
    {
        frame.BuildInternal(AssemblyInfo.Get(Assembly.GetCallingAssembly()).plugin);
        return frame;
    }
}

namespace GrooveSharedUtils.Frames
{
    public abstract class BaseFrame : IEnumerable
    {

        protected abstract IEnumerable<object> Assets { get; }
        /*public T Build()
        {
            BuildInternal(AssemblyInfo.Get(Assembly.GetCallingAssembly()).plugin);
            return this as T;
        }*/

        public IEnumerator GetEnumerator()
        {
            return Assets.GetEnumerator();
        }
        protected internal abstract void BuildInternal(BaseModPlugin callingMod);
    }
}
