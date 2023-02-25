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
using GrooveSharedUtils.ScriptableObjects;
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using HG;
using BepInEx.Configuration;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;
using RoR2.ExpansionManagement;

namespace GrooveSharedUtils
{
    public class UpdateItemFollowerScaleDebug : MonoBehaviour
    {
        private ItemFollower itemFollower;
        public void Awake()
        {
            itemFollower = base.GetComponent<ItemFollower>();
        }
        public void Update()
        {
            if (itemFollower && itemFollower.followerInstance)
            {
                itemFollower.followerInstance.transform.localScale = itemFollower.transform.localScale;
            }
        }
    }

}
