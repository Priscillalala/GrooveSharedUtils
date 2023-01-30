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
    public class AssetFieldLocator : Dictionary<(string, Type), FieldInfo>
    {
        public AssetFieldLocator() : base(sharedComparer) { }
        public static AssetComparer sharedComparer = new AssetComparer();

        public class AssetComparer : IEqualityComparer<(string, Type)>
        {
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
                if (x2 != y2)
                {
                    return false;
                }
                x1 = WithoutSpaces(x1);
                y1 = WithoutSpaces(y1);
                return x1.Length == y1.Length && string.Compare(x1, y1, StringComparison.OrdinalIgnoreCase) == 0;
            }
            //https://stackoverflow.com/questions/2182459/fastest-way-to-remove-chars-from-string
            public static string WithoutSpaces(string s)
            {
                int len = s.Length;
                char[] s2 = new char[len];
                int i2 = 0;
                for (int i = 0; i < len; i++)
                {
                    char c = s[i];
                    if (!char.IsWhiteSpace(c) && c != '.' && c != '_')
                        s2[i2++] = c;
                }
                return new string(s2, 0, i2);
            }
            public int GetHashCode((string, Type) obj)
            {
                return CombineHashCodes(StringComparer.OrdinalIgnoreCase.GetHashCode(WithoutSpaces(obj.Item1)), obj.Item2.GetHashCode());
            }
            public static int CombineHashCodes(int h1, int h2)
            {
                return (h1 << 5) + h1 ^ h2;
            }
        }
    }
}
