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

namespace GrooveSharedUtils.Frames
{
    public abstract class BaseFrame<T> : IEnumerable where T : BaseFrame<T>
    {
        internal abstract object[] Assets { get; }
        public T Build()
        {
            BuildInternal();
            return this as T;
        }
        public IEnumerator GetEnumerator()
        {
            return Assets.GetEnumerator();
        }
        internal abstract void BuildInternal();
    }
}
