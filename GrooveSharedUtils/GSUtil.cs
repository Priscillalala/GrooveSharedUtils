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
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using RoR2.ContentManagement;
using System.Collections;
using System.Text;
using BepInEx.Configuration;
using HG;
using UnityEngine.Networking;

namespace GrooveSharedUtils
{
    public static class GSUtil
    {
        public static TAsset LegacyLoad<TAsset>(string path) where TAsset : UnityEngine.Object
        {
            return LegacyResourcesAPI.Load<TAsset>(path);
        }
        public static TAsset AddressablesLoad<TAsset>(string key) where TAsset : UnityEngine.Object
        {
            return Addressables.LoadAssetAsync<TAsset>(key).WaitForCompletion();
        }
        public static void Destroy(UnityEngine.Object obj)
        {
            UnityEngine.Object.Destroy(obj);
        }
        public static void DestroyImmediate(UnityEngine.Object obj)
        {
            UnityEngine.Object.DestroyImmediate(obj);
        }
        public static Type[] GetEntityStateTypes(Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            return assembly.GetTypes().Where((Type t) => typeof(EntityStates.EntityState).IsAssignableFrom(t)).ToArray();
        }
        public static T[] Array<T>(params T[] values)
        {
            return values;
        }
        public static Color ColorRGB(float rUnscaled, float gUnscaled, float bUnscaled, float a = 1)
        {
            return new Color(rUnscaled / 255f, gUnscaled / 255f, bUnscaled / 255f, a);
        }
        public static bool HasItem(this CharacterBody characterBody, ItemDef itemDef, out int stack)
        {
            return HasItem(characterBody, itemDef.itemIndex, out stack);
        }
        public static bool HasItem(this CharacterBody characterBody, ItemIndex itemIndex, out int stack)
        {
            if (characterBody && characterBody.inventory)
            {
                stack = characterBody.inventory.GetItemCount(itemIndex);
                return true;
            }
            stack = 0;
            return false;
        }
        public static bool HasItem(this CharacterMaster characterMaster, ItemDef itemDef, out int stack)
        {
            return HasItem(characterMaster, itemDef.itemIndex, out stack);
        }
        public static bool HasItem(this CharacterMaster characterMaster, ItemIndex itemIndex, out int stack)
        {
            if (characterMaster && characterMaster.inventory)
            {
                stack = characterMaster.inventory.GetItemCount(itemIndex);
                return true;
            }
            stack = 0;
            return false;
        }

        public static float StackScaling(float baseValue, float stackValue, int stack)
        {
            if (stack > 0)
            {
                return baseValue + ((stack - 1) * stackValue);
            }
            return 0f;
        }
        public static int StackScaling(int baseValue, int stackValue, int stack)
        {
            if (stack > 0)
            {
                return baseValue + ((stack - 1) * stackValue);
            }
            return 0;
        }
        public static GameObject EmptyPrefab(string name, bool registerNetwork = false)
        {
            GameObject g = new GameObject(name);
            if (registerNetwork)
            {
                g.AddComponent<NetworkIdentity>();
            }
            return PrefabAPI.InstantiateClone(g, name, registerNetwork);
        }
        public static string FormatCharacters(this string original, Func<char, bool> predicate)
        {
            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            for (int i = 0; i < original.Length; i++)
            {
                char c = original[i];
                if (predicate.Invoke(c))
                {
                    stringBuilder.Append(c);
                }
            }
            string result = stringBuilder.ToString();
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            return result;
        }
        public static string InternalToExternalName(string internalName)
        {
            internalName = internalName.Trim();
            int length = internalName.Length;
            if (length <= 1)
            {
                return internalName;
            }
            char[] chars = internalName.ToCharArray();
            ref char c = ref chars[0];
            c = char.ToUpper(c);

            int i = 1;
            int offset = 0;
            while (i < length)
            {
                if (char.IsUpper(internalName[i]))
                {
                    char prevChar = internalName[i - 1];
                    if (char.IsLower(prevChar) && !char.IsWhiteSpace(prevChar))
                    {
                        ArrayUtils.ArrayInsert(ref chars, i + offset, ' ');
                        offset++;
                    }
                    else
                    {
                        int nextIndex = i + 1;
                        if (nextIndex < length)
                        {
                            char nextChar = internalName[nextIndex];
                            if (char.IsLower(nextChar) && !char.IsWhiteSpace(nextChar))
                            {
                                ArrayUtils.ArrayInsert(ref chars, i + offset, ' ');
                                offset++;
                            }
                        }
                    }
                }
                i++;
            }
            return new string(chars);
        }
        public static void ClearDotStacksForType(this DotController dotController, DotController.DotIndex dotIndex)
        {
            for(int i = dotController.dotStackList.Count - 1; i >= 0; i--)
            {
                if(dotController.dotStackList[i].dotIndex == dotIndex)
                {
                    dotController.RemoveDotStackAtServer(i);
                }
            }
        }
        public static bool IsSpecialCharacter(this char character)
        {
            return specialCharacters.Contains(character);
        }
        public static readonly char[] specialCharacters = new char[]
        {
            '=',
            '\n',
            '\t',
            '\\',
            '"',
            '\'',
            '[',
            ']',
        };
        public static void AddLang(this string token, string value, string specificLanguage = null)
        {
            if (string.IsNullOrEmpty(specificLanguage))
            {
                LanguageAPI.Add(token, value);
                return;
            }
            LanguageAPI.Add(token, value, specificLanguage);
        }
        public static string EnsurePrefix(this string str, string prefix)
        {
            if (str.StartsWith(prefix))
            {
                return str;
            }
            return string.Concat(prefix, str);
        }
        public static void CopyTo<T>(this T src, T dest, bool copyFields = true, bool copyProperties = false) where T : ScriptableObject
        {
            if (copyFields)
            {
                FieldInfo[] fields = typeof(T).GetFields();
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo fieldInfo = fields[i];
                    fieldInfo.SetValue(dest, fieldInfo.GetValue(src));
                }
            }
            if (copyProperties)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo propertyInfo = properties[i];
                    if (propertyInfo.CanRead && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(dest, propertyInfo.GetValue(src));
                    }
                }
            }
        }
        public static void Log(LogLevel level, object data)
        {
            LogInternal(level, data, Assembly.GetCallingAssembly());
        }
        public static void Log(object data)
        {
            LogInternal(LogLevel.Info, data, Assembly.GetCallingAssembly());
        }
        internal static void LogInternal(LogLevel level, object data, Assembly callingAssembly)
        {
            BaseModPlugin plugin = AssemblyInfo.Get(callingAssembly).plugin;
            if (plugin)
            {
                plugin.Logger.Log(level, data);
            }
        }
        internal static ConditionalWeakTable<NamedAssetCollection, HashSet<object>> internalAssetCollectionToHash = new ConditionalWeakTable<NamedAssetCollection, HashSet<object>>();
        internal struct ResolveHashTypeInfo
        {
            public MethodInfo add;
            public MethodInfo ofType;
            public MethodInfo toArray;
        }
        internal static Dictionary<Type, ResolveHashTypeInfo> cachedResolvedHashInfo = new Dictionary<Type, ResolveHashTypeInfo>();
        public static void AddHash(this NamedAssetCollection namedAssetCollection, object asset)
        {
            HashSet<object> hashSet = internalAssetCollectionToHash.GetOrCreateValue(namedAssetCollection);
            hashSet.Add(asset);
        }
        /*public static void AddHash<TAsset>(this NamedAssetCollection<TAsset> namedAssetCollection, TAsset asset)
        {
            HashSet<object> hashSet = internalAssetCollectionToHash.GetOrCreateValue(namedAssetCollection);
            hashSet.Add(asset);
        }*/
        public static void ResolveHashDisgusting(this NamedAssetCollection namedAssetCollection, Type asType)
        {
            if (internalAssetCollectionToHash.TryGetValue(namedAssetCollection, out HashSet<object> hashSet))
            {
                ResolveHashTypeInfo info;
                if (!cachedResolvedHashInfo.TryGetValue(asType, out info))
                {
                    info = new ResolveHashTypeInfo
                    {
                        ofType = typeof(Enumerable).GetMethod("OfType", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(asType),
                        toArray = typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(asType),
                        add = typeof(NamedAssetCollection<>).MakeGenericType(asType).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public),
                    };
                    cachedResolvedHashInfo.Add(asType, info);
                }
                object genericHashSet = info.ofType.Invoke(null, new[] { hashSet });
                object genericArray = info.toArray.Invoke(null, new[] { genericHashSet });
                info.add.Invoke(namedAssetCollection, new[] { genericArray });
                internalAssetCollectionToHash.Remove(namedAssetCollection);
            }
        }
        public static void ResolveHash<TAsset>(this NamedAssetCollection<TAsset> namedAssetCollection)
        {
            if (internalAssetCollectionToHash.TryGetValue(namedAssetCollection, out HashSet<object> hashSet))
            {
                namedAssetCollection.Add(hashSet.OfType<TAsset>().ToArray());
                internalAssetCollectionToHash.Remove(namedAssetCollection);
            }
        }
        public static TValue GetOrCreateValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> createValueDelegate = null)
        {
            if (!dict.TryGetValue(key, out TValue value))
            {
                value = createValueDelegate != null ? createValueDelegate.Invoke() : Activator.CreateInstance<TValue>();
                dict.Add(key, value);
            }
            return value;
        }
        public static ConfigFile GetOrCreateConfig(string name, BepInPlugin owner = null)
        {
            return GetOrCreateConfig(name, Assembly.GetCallingAssembly(), owner);
        }
        public static ConfigFile GetOrCreateConfig(string name, Assembly assembly, BepInPlugin owner = null)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return null;
            }
            Dictionary<string, ConfigFile> configFiles = AssemblyInfo.Get(assembly).configFiles;
            return configFiles.GetOrCreateValue(name, () =>
            {
                string path = System.IO.Path.Combine(Paths.ConfigPath, name + ".cfg");
                ConfigFile configFile = new ConfigFile(path, true, owner);
                AddDisplayAsset(configFile, assembly);
                return configFile;
            });

        }
        public static void AddDisplayAsset<TAsset>(TAsset asset, Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            AssemblyInfo assemblyInfo = AssemblyInfo.Get(assembly);
            if (assemblyInfo.plugin)
            {
                assemblyInfo.plugin.AddDisplayAsset(asset);
                return;
            }
            assemblyInfo.pendingDisplayAssets.Enqueue(asset);
        }
    }
}
