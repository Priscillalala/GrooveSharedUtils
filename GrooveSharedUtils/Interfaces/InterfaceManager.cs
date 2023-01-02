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

namespace GrooveSharedUtils.Interfaces
{
    public static class InterfaceManager
    {
        internal static void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            List<IOnGetStatCoefficientsReciever> getStatCoefficientsRecievers = GetComponentsCache<IOnGetStatCoefficientsReciever>.GetGameObjectComponents(sender.gameObject);
            foreach (IOnGetStatCoefficientsReciever reciever in getStatCoefficientsRecievers)
            {
                reciever.OnGetStatCoefficients(args);
            }
            GetComponentsCache<IOnGetStatCoefficientsReciever>.ReturnBuffer(getStatCoefficientsRecievers);
        }
    }
        
}
