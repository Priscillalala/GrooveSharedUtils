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
using RoR2;

namespace GrooveSharedUtils
{
    public struct LazyAddressable<T>  where T : UnityEngine.Object
    {
        public AsyncOperationHandle<T> operation { get; private set; }
        public T Result => operation.Result;
        public bool IsCompleted => operation.IsDone;

        public event Action<T> OnCompleted 
        {
            add => operation.Completed += handle => value(handle.Result);
            remove => operation.Completed -= handle => value(handle.Result);
        }
        public event Action<object> OnCompletedTypeless
        {
            add => operation.CompletedTypeless += handle => value(handle.Result);
            remove => operation.CompletedTypeless -= handle => value(handle.Result);
        }
        public static implicit operator T(LazyAddressable<T> lazyAddressable)
        {
            return lazyAddressable.WaitForCompletion();
        }
        public static implicit operator LazyAddressable<T>(string key)
        {
            return new LazyAddressable<T>(key);
        }
        public LazyAddressable(string key, bool ensureCompletion = true)
        {
            operation = default(AsyncOperationHandle<T>);
            try
            {
                operation = Addressables.LoadAssetAsync<T>(key.Trim());
            }
            catch (Exception e)
            {
                GroovyLogger.Log(BepInEx.Logging.LogLevel.Error, e.ToString());
            }
            if(ensureCompletion && operation.IsValid())
            {
                addressablesOperations.Add(operation);
            }
        }
        public T WaitForCompletion()
        {
            operation.WaitForCompletion();
            return Result;
        }

        static List<AsyncOperationHandle> addressablesOperations = new List<AsyncOperationHandle>();
        static LazyAddressable()
        {
            On.RoR2.RoR2Application.Awake += RoR2Application_Awake;
        }
        static void RoR2Application_Awake(On.RoR2.RoR2Application.orig_Awake orig, RoR2Application self)
        {
            On.RoR2.RoR2Application.Awake -= RoR2Application_Awake;
            if (addressablesOperations != null)
            {
                foreach (AsyncOperationHandle asyncOperationHandle in addressablesOperations)
                {
                    if (asyncOperationHandle.IsDone) continue;
                    try
                    {
                        asyncOperationHandle.WaitForCompletion();
                    }
                    catch (Exception e)
                    {
                        GroovyLogger.Log(BepInEx.Logging.LogLevel.Error, e.ToString());
                    }
                }
                addressablesOperations = null;
            }
            orig(self);
        }
    }
}
    

