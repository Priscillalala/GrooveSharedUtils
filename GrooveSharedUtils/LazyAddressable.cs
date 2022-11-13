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
        public bool neverCached;

        AsyncOperationHandle<T> loadOperation;

        T _cachedObject;

        public static implicit operator T(LazyAddressable<T> lazyAddressable)
        {
            if (lazyAddressable._cachedObject)
            {
                return lazyAddressable._cachedObject;
            }
            lazyAddressable.RequestLoadAsync();
            return lazyAddressable._cachedObject = lazyAddressable.loadOperation.WaitForCompletion();
        }
        public static implicit operator LazyAddressable<T>(string key)
        {
            return new LazyAddressable<T>
            {
                key = key
            };
        }

        public void RequestLoadAsync()
        {
            if (_cachedObject || loadOperation.IsValid())
            {
                return;
            }
            loadOperation = Addressables.LoadAssetAsync<T>(key);
        }

    }
}
