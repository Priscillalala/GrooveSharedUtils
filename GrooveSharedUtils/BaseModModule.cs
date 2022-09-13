﻿using BepInEx;
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

namespace GrooveSharedUtils
{

    public abstract class BaseModModule<T> : BaseModModule
    {
        public static T instance { get; private set; }
        public BaseModModule()
        {
            if(instance != null)
            {
                return;
            }
            if(this is T t)
            {
                instance = t;
            }
        }
    }
    public abstract class BaseModModule
    {
        public abstract void OnModInit();
        public abstract void OnCollectContent(AssetStream sasset);
    }
}
