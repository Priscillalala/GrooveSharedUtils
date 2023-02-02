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
using RoR2.Skills;
using HG.GeneralSerializer;

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
        public static Type[] GetEntityStateTypes(Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            return assembly.GetTypes().Where((Type t) => typeof(EntityStates.EntityState).IsAssignableFrom(t)).ToArray();
        }
        public static T[] Array<T>(params T[] values) => values;
        public static Color ColorRGB(float rUnscaled, float gUnscaled, float bUnscaled, float a = 1)
        {
            return new Color(rUnscaled / 255f, gUnscaled / 255f, bUnscaled / 255f, a);
        }
        public static AssetBundle LoadAssetBundleRelative(string relativePath = "", Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            return AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), relativePath));
        }
        public static AssetBundleCreateRequest LoadAssetBundleRelativeAsync(string relativePath = "", Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            return AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), relativePath));
        }
        #region HasItem
        //with stack
        public static bool HasItem(this CharacterBody characterBody, ItemDef itemDef, out int stack) => HasItem(characterBody, itemDef ? itemDef.itemIndex : ItemIndex.None, out stack);
        public static bool HasItem(this CharacterBody characterBody, ItemIndex itemIndex, out int stack)
        {
            if (characterBody && characterBody.inventory)
            {
                stack = characterBody.inventory.GetItemCount(itemIndex);
                return stack > 0;
            }
            stack = 0;
            return false;
        }
        public static bool HasItem(this CharacterMaster characterMaster, ItemDef itemDef, out int stack) => HasItem(characterMaster, itemDef ? itemDef.itemIndex : ItemIndex.None, out stack);
        public static bool HasItem(this CharacterMaster characterMaster, ItemIndex itemIndex, out int stack)
        {
            if (characterMaster && characterMaster.inventory)
            {
                stack = characterMaster.inventory.GetItemCount(itemIndex);
                return stack > 0;
            }
            stack = 0;
            return false;
        }
        //no stack
        public static bool HasItem(this CharacterBody characterBody, ItemDef itemDef) => HasItem(characterBody, itemDef.itemIndex);
        public static bool HasItem(this CharacterBody characterBody, ItemIndex itemIndex) => characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(itemIndex) > 0;
        public static bool HasItem(this CharacterMaster characterMaster, ItemDef itemDef) => HasItem(characterMaster, itemDef.itemIndex);
        public static bool HasItem(this CharacterMaster characterMaster, ItemIndex itemIndex) => characterMaster && characterMaster.inventory && characterMaster.inventory.GetItemCount(itemIndex) > 0;
        #endregion
        public static SkillFamily FindSkillFamily(GameObject bodyPrefab, SkillSlot slot)
        {
            if(bodyPrefab.TryGetComponent(out SkillLocator skillLocator))
            {
                return skillLocator.GetSkill(slot)?.skillFamily;
            }
            return null;
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
        public static ModelPanelParameters SetupModelPanelParameters(GameObject model, Vector3 modelRotation, float minDistance, float maxDistance, Transform focusPoint = null, Transform cameraPosition = null)
        {
            ModelPanelParameters parameters = model.AddComponent<ModelPanelParameters>();
            parameters.modelRotation = Quaternion.Euler(modelRotation);// Util.QuaternionSafeLookRotation(Vector3. modelRotation);
            parameters.minDistance = minDistance;
            parameters.maxDistance = maxDistance;
            parameters.focusPointTransform = focusPoint ?? model.transform.Find("FocusPoint") ?? model.transform.Find("Focus Point");
            parameters.cameraPositionTransform = cameraPosition ?? model.transform.Find("CameraPosition") ?? model.transform.Find("Camera Position");
            return parameters;
        }
        /*public static string FormatCharacters(this string original, Func<char, bool> predicate)
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
        }*/
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
        #region ModLoaded
        public static bool ModLoaded(string guid)
        {
            if (!Chainloader._loaded)
            {
                Debug.LogWarning($"Cannot determine if {guid} is loaded, the Chainloader has not yet loaded!");
                return false;
            }
            return Chainloader.PluginInfos.ContainsKey(guid);
        }
        internal static Dictionary<string, bool> modLoadedCache = new Dictionary<string, bool>();
        public static bool ModLoadedCached(string guid)
        {
            if (!Chainloader._loaded)
            {
                Debug.LogWarning($"Cannot determine if {guid} is loaded, the Chainloader has not yet loaded!");
                return false;
            }
            return modLoadedCache.GetOrCreateValue(guid, () => Chainloader.PluginInfos.ContainsKey(guid));
        }
        #endregion
        public static bool TryModifyFieldValue<T>(this EntityStateConfiguration entityStateConfiguration, string fieldName, T value)
        {
            ref SerializedField serializedField = ref entityStateConfiguration.serializedFieldsCollection.GetOrCreateField(fieldName);
            Type type = typeof(T);
            if (serializedField.fieldValue.objectValue && typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                serializedField.fieldValue.objectValue = value as UnityEngine.Object;
                return true;
            }
            else if (serializedField.fieldValue.stringValue != null && StringSerializer.CanSerializeType(type))
            {
                serializedField.fieldValue.stringValue = StringSerializer.Serialize(type, value);
                return true;
            }
            return false;
        }
        public static void Add(this ItemDisplayRuleSet idrs, UnityEngine.Object keyAsset, GameObject displayPrefab, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale, LimbFlags limbMask = LimbFlags.None)
        {
            Add(idrs, keyAsset, new ItemDisplayRule
            {
                followerPrefab = displayPrefab,
                childName = childName,
                localPos = localPos,
                localAngles = localAngles,
                localScale = localScale,
                limbMask = limbMask,
                ruleType = limbMask > LimbFlags.None ? ItemDisplayRuleType.LimbMask : ItemDisplayRuleType.ParentedPrefab
            });
        }
        public static void Add(this ItemDisplayRuleSet idrs, UnityEngine.Object keyAsset, ItemDisplayRule rule)
        {
            if (!keyAsset) return;
            for (int i = 0; i < idrs.keyAssetRuleGroups.Length; i++)
            {
                if (idrs.keyAssetRuleGroups[i].keyAsset == keyAsset)
                {
                    idrs.keyAssetRuleGroups[i].displayRuleGroup.AddDisplayRule(rule);
                    return;
                }
            }
            ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = keyAsset,
            };
            keyAssetRuleGroup.displayRuleGroup.AddDisplayRule(rule);
            ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, keyAssetRuleGroup);
        }
        public static void EnsurePrefix(ref string str, string prefix)
        {
            if(str != null && !str.StartsWith(prefix))
            {
                str = string.Concat(prefix, str);
            }
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
        public static void LogDebug(LogLevel level, object data, Assembly explicitAssembly = null)
        {
            Assembly callingAssemblyInfo = explicitAssembly ?? Assembly.GetCallingAssembly();
            if (ModPlugin.TryFind(callingAssemblyInfo, out ModPlugin plugin) && plugin.isDebug)
            {
                LogInternal(level, data, callingAssemblyInfo);
            }
        }
        public static void LogDebug(object data)
        {
            LogDebug(LogLevel.Debug, data, Assembly.GetCallingAssembly());
        }
        public static void Log(LogLevel level, object data, Assembly explicitAssembly = null)
        {
            LogInternal(level, data, explicitAssembly ?? Assembly.GetCallingAssembly());
        }
        public static void Log(object data)
        {
            Log(LogLevel.Info, data, Assembly.GetCallingAssembly());
        }
        internal static void LogInternal(LogLevel level, object data, Assembly assembly)
        {
            ManualLogSource logger = null;
            if (ModPlugin.TryFind(assembly, out ModPlugin plugin))
            {
                logger = plugin.Logger;
            }
            else
            {
                logger = Chainloader.PluginInfos.Values.FirstOrDefault(info => info.Instance && info.Instance.GetType().Assembly == assembly)?.Instance.Logger;
            }
            if (logger != null)
            {
                logger.Log(level, data);
            }
            else
            {
                Debug.Log(data);
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
        public static bool TryFreeValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            if (dict.TryGetValue(key, out value))
            {
                dict.Remove(key);
                return true;
            }
            return false;
        }
    }
}
