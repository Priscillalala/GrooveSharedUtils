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
    public static class LanguageCollectionManager
    {
        /*public struct LanguageString
        {
            public LanguageString(string token, string localizedString)
            {
                //string a = token >> localizedString;
                this.token = token;
                this.localizedString = localizedString;
            }
            public string token;
            public string localizedString;
            public static implicit operator (string, string)(LanguageString languageString) => (languageString.token, languageString.localizedString);
            public static implicit operator LanguageString((string, string)tuple) => new LanguageString(tuple.Item1, tuple.Item2);
            public static implicit operator LanguageString(string s) => new LanguageString(s, s);
            public static LanguageString operator -(LanguageString a, LanguageString b) => a;
        }*/
        /*public class LanguageCollection : List<(string token, string localizedString)>
        {
            public static implicit operator LanguageCollection((string, string)[] array) => (LanguageCollection)array.ToList();
        }*/
        /*[LanguageCollectionProvider(languageNameOverride = "en")]
        public static LanguageCollection Items() => new []        
        { 
            ("ITEM_IMMUNETODEBUFF_NAME", "Ben's Aegis"),
            ("ITEM_IMMUNETODEBUFF_LORE", "Grrrr... this item SUCKS!"),
            ("COMMANDO_BODY_NAME", $"Test string...{5+7:P}"),
        };*/

        public static Dictionary<string, List<MethodInfo>> languageToProviders = new Dictionary<string, List<MethodInfo>>(); 
        public static void Init()
        {
            Type expectedReturnType = typeof(LanguageCollection);
            List<LanguageCollectionProviderAttribute> languageCollectionProviderAttributes = new List<LanguageCollectionProviderAttribute>();
            HG.Reflection.SearchableAttribute.GetInstances(languageCollectionProviderAttributes);
            foreach(LanguageCollectionProviderAttribute provider in languageCollectionProviderAttributes)
            {
                if(provider.target is MethodInfo methodInfo)
                {
                    if (!methodInfo.IsStatic)
                    {
                        GSUtil.Log(LogLevel.Warning, $"{nameof(LanguageCollectionProviderAttribute)} cannot target method {methodInfo.Name}: it is not static!");
                        continue;
                    }
                    if (!expectedReturnType.IsAssignableFrom(methodInfo.ReturnType))
                    {
                        string returnType = methodInfo.ReturnType != null ? methodInfo.ReturnType.FullName : "void";
                        GSUtil.Log(LogLevel.Warning, $"{nameof(LanguageCollectionProviderAttribute)} cannot target method {methodInfo.Name}: return type is {returnType}, and needs to be {expectedReturnType.Name}!");
                        continue;
                    }
                    if(methodInfo.GetGenericArguments().Length != 0)
                    {
                        GSUtil.Log(LogLevel.Warning, $"{nameof(LanguageCollectionProviderAttribute)} cannot target method {methodInfo.Name}: it has arguments!");
                        continue;
                    }
                    string languageName = string.IsNullOrEmpty(provider.languageNameOverride) ? methodInfo.DeclaringType.Name : provider.languageNameOverride;
                    languageToProviders.GetOrCreateValue(languageName).Add(methodInfo);
                }
                
            }
            On.RoR2.Language.SetStringsByTokens += Language_SetStringsByTokens;

        }

        private static void Language_SetStringsByTokens(On.RoR2.Language.orig_SetStringsByTokens orig, Language self, IEnumerable<KeyValuePair<string, string>> tokenPairs)
        {
            orig(self, tokenPairs);
            if(languageToProviders.TryGetValue(Language.currentLanguageName, out List<MethodInfo> providers))
            {
                foreach(MethodInfo provider in providers)
                {
                    if (provider.Invoke(null, Array.Empty<object>()) is LanguageCollection collection)
                    {
                        for(int i = 0; i < collection.Count; i++)
                        {
                            self.SetStringByToken(collection[i].token, collection[i].localizedString);
                        }
                    }
                }
            }
        }
    }
        
}
