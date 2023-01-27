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
using JetBrains.Annotations;
using BepInEx.Logging;
using System.Globalization;

namespace GrooveSharedUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ConditionalRegisterAchievementAttribute : HG.Reflection.SearchableAttribute
    {
        public string identifier;
        public string unlockableRewardIdentifier;
        public string prerequisiteAchievementIdentifier;
        public Type serverTrackerType;
        public ConditionalRegisterAchievementAttribute([NotNull] string identifier, string unlockableRewardIdentifier, string prerequisiteAchievementIdentifier = null, Type serverTrackerType = null)
        {
            this.identifier = identifier;
            this.unlockableRewardIdentifier = unlockableRewardIdentifier;
            this.prerequisiteAchievementIdentifier = prerequisiteAchievementIdentifier;
            this.serverTrackerType = serverTrackerType;
        }
        internal static void Init()
        {
            RoR2BepInExPack.VanillaFixes.SaferAchievementManager.OnCollectAchievementDefs += SaferAchievementManager_OnCollectAchievementDefs;
        }

        internal static void SaferAchievementManager_OnCollectAchievementDefs(List<string> identifiers, Dictionary<string, AchievementDef> identifierToAchievementDef, List<AchievementDef> achivementDefs)
        {
            Type expectedReturnType = typeof(bool);
            List<ConditionalRegisterAchievementAttribute> attributes = new List<ConditionalRegisterAchievementAttribute>();
            GetInstances(attributes);
            foreach (ConditionalRegisterAchievementAttribute attribute in attributes)
            {
                if (attribute.target is MethodInfo methodInfo)
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
                    if (methodInfo.GetGenericArguments().Length != 0)
                    {
                        GSUtil.Log(LogLevel.Warning, $"{nameof(LanguageCollectionProviderAttribute)} cannot target method {methodInfo.Name}: it has arguments!");
                        continue;
                    }
                    if (methodInfo.Invoke(null, Array.Empty<object>()) is bool shouldRegister && shouldRegister)
                    {
                        if (identifierToAchievementDef.ContainsKey(attribute.identifier))
                        {
                            GSUtil.Log(LogLevel.Warning, $"Class {methodInfo.DeclaringType.FullName} attempted to register as achievement {attribute.identifier}, but class {identifierToAchievementDef[attribute.identifier].type.FullName} has already registered as that achievement.");
                        } 
                        else
                        {
                            AchievementDef achievementDef = AchievementDefFromAttribute(attribute, methodInfo.DeclaringType);
                            identifiers.Add(achievementDef.identifier);
                            identifierToAchievementDef.Add(achievementDef.identifier, achievementDef);
                            achivementDefs.Add(achievementDef);
                        }
                    }
                }

            }
        }
        internal static AchievementDef AchievementDefFromAttribute(ConditionalRegisterAchievementAttribute attribute, Type type)
        {
            UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(attribute.unlockableRewardIdentifier);
            AchievementDef achievementDef = new AchievementDef
            {
                identifier = attribute.identifier,
                unlockableRewardIdentifier = attribute.unlockableRewardIdentifier,
                prerequisiteAchievementIdentifier = attribute.prerequisiteAchievementIdentifier,
                nameToken = "ACHIEVEMENT_" + attribute.identifier.ToUpper(CultureInfo.InvariantCulture) + "_NAME",
                descriptionToken = "ACHIEVEMENT_" + attribute.identifier.ToUpper(CultureInfo.InvariantCulture) + "_DESCRIPTION",
                type = type,
                serverTrackerType = attribute.serverTrackerType
            };
            if (unlockableDef && unlockableDef.achievementIcon)
            {
                achievementDef.SetAchievedIcon(unlockableDef.achievementIcon);
            }
            else
            {
                achievementDef.iconPath = "Textures/AchievementIcons/tex" + attribute.identifier + "Icon";
            }
            if (unlockableDef != null)
            {
                unlockableDef.getHowToUnlockString = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                {
                    Language.GetString(achievementDef.nameToken),
                    Language.GetString(achievementDef.descriptionToken)
                });
                unlockableDef.getUnlockedString = () => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                {
                    Language.GetString(achievementDef.nameToken),
                    Language.GetString(achievementDef.descriptionToken)
                });
            }
            return achievementDef;
        }
    }
}
