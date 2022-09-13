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
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using System.Runtime.CompilerServices;
using System.Collections;
using RoR2.ContentManagement;
using UnityEngine.Events;
using System.Globalization;

namespace GrooveSharedUtils
{
    public class AssetFieldLocatorComparer : IEqualityComparer<(string, Type)>
    {
        public static AssetFieldLocatorComparer comparer = new AssetFieldLocatorComparer();
        public bool Equals((string, Type) x, (string, Type) y)
        {
            if (x == y)
            {
                return true;
            }
            string x1 = x.Item1;
            Type x2 = x.Item2;
            string y1 = y.Item1;
            Type y2 = y.Item2;
            if (x1 == null || x2 == null || y1 == null || y2 == null)
            {
                return false;
            }
            return x2 == y2 && x1.Length == y1.Length && string.Compare(x1, y1, StringComparison.OrdinalIgnoreCase) == 0; ;
        }
        public int GetHashCode((string, Type) obj)
        {
            return CombineHashCodes(StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item1), obj.Item2.GetHashCode());
        }
        public static int CombineHashCodes(int h1, int h2)
        {
            return (h1 << 5) + h1 ^ h2;
        }
    }
}
