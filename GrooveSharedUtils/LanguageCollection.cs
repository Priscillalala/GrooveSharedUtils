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
using GrooveSharedUtils.Attributes;
using BepInEx.Logging;

namespace GrooveSharedUtils
{
    public class LanguageCollection : List<(string token, string localizedString)>
    {
        public static implicit operator LanguageCollection((string, string)[] array) => (LanguageCollection)array.ToList();
    }
}
