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
using R2API.ScriptableObjects;
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using System.Runtime.CompilerServices;
using System.Collections;
using RoR2.ContentManagement;
using UnityEngine.Events;
using BepInEx.Configuration;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GrooveSharedUtils
{
    public struct LazyAddressable<T> where T : UnityEngine.Object
    {
        public string key;

        T _cachedObject;

        public static implicit operator T(LazyAddressable<T> lazyAddressable)
        {
            if (lazyAddressable._cachedObject)
            {
                return lazyAddressable._cachedObject;
            }
            if (string.IsNullOrEmpty(lazyAddressable.key))
            {
                return null;
            }
            AsyncOperationHandle<T> asyncOperationHandle = default;
            try
            {
                asyncOperationHandle = Addressables.LoadAssetAsync<T>(lazyAddressable.key);
            }
            finally
            {
                if (asyncOperationHandle.IsValid())
                {
                    lazyAddressable._cachedObject = asyncOperationHandle.WaitForCompletion();
                }
            }
            return lazyAddressable._cachedObject;
        }
        public static implicit operator LazyAddressable<T>(string key)
        {
            return new LazyAddressable<T>
            {
                key = key
            };
        }
        public LazyAddressable(string key)
        {
            this.key = key;
            this._cachedObject = null;
        }
        

    }
}
