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
using R2API;
using UnityEngine.AddressableAssets;
using HG;
using RoR2.ExpansionManagement;
using JetBrains.Annotations;
using System.Collections;
using RoR2.Achievements;
using GrooveSharedUtils.Frames;

namespace GrooveSharedUtils.Frames
{
    public class AchievementFrame : AchievementFrame<AchievementFrame, AchievementDef, UnlockableDef> { }
    public class AchievementFrame<TAchievementDef, TUnlockableDef> : AchievementFrame<AchievementFrame<TAchievementDef, TUnlockableDef>, TAchievementDef, TUnlockableDef>
        where TAchievementDef : AchievementDef
        where TUnlockableDef : UnlockableDef { }
    //public class AchievementFrame<TBaseAchievement, TServerAchievement> : AchievementFrame<TBaseAchievement, TServerAchievement, AchievementDef, UnlockableDef> where TBaseAchievement : BaseAchievement where TServerAchievement : BaseServerAchievement { }

    public abstract class AchievementFrame<TFrame, TAchievementDef, TUnlockableDef> : Frame<TFrame> 
        where TFrame : AchievementFrame<TFrame, TAchievementDef, TUnlockableDef>
        where TAchievementDef : AchievementDef 
        where TUnlockableDef : UnlockableDef
    {
        public string identifier;
        public string unlockableRewardName;
        public string prerequisiteAchievementIdentifier = null;
        public Type trackerType = null;
        public Type serverTrackerType = null;
        public string overrideAchievementNameToken = null;
        public string overrideAchievementDescriptionToken = null;
        public string overrideUnlockableNameToken = null;
        public Sprite achievementIcon;
        public TAchievementDef AchievementDef { get; private set; }
        public TUnlockableDef UnlockableDef { get; private set; }
        public TFrame SetTrackerType<TBaseAchievement>() 
            where TBaseAchievement : BaseAchievement
        {
            trackerType = typeof(TBaseAchievement);
            return this as TFrame;
        }
        public TFrame SetServerTrackerType<TBaseServerAchievement>() 
            where TBaseServerAchievement : BaseServerAchievement
        {
            serverTrackerType = typeof(TBaseServerAchievement);
            return this as TFrame;
        }
        public TFrame SetTrackerTypes<TBaseAchievement, TBaseServerAchievement>() 
            where TBaseAchievement : BaseAchievement 
            where TBaseServerAchievement : BaseServerAchievement
        {
            SetTrackerType<TBaseAchievement>();
            SetServerTrackerType<TBaseServerAchievement>();
            return this as TFrame;
        }
        protected override IEnumerable GetAssets()
        {
            yield return AchievementDef;
            yield return UnlockableDef;
        }
        protected internal override void BuildInternal([CanBeNull] ModPlugin callingMod)
        {
            string token = identifier.ToUpperInvariant();
            string tokenPrefix = callingMod ? callingMod.adjustedGeneratedTokensPrefix : string.Empty;
            AchievementDef = Activator.CreateInstance<TAchievementDef>();
            AchievementDef.identifier = identifier;
            AchievementDef.unlockableRewardIdentifier = unlockableRewardName;
            AchievementDef.prerequisiteAchievementIdentifier = prerequisiteAchievementIdentifier;
            AchievementDef.nameToken = overrideAchievementNameToken ?? $"{tokenPrefix}ACHIEVEMENT_{token}_NAME";
            AchievementDef.descriptionToken = overrideAchievementDescriptionToken ?? $"{tokenPrefix}ACHIEVEMENT_{token}_NAME";
            AchievementDef.type = trackerType;
            AchievementDef.serverTrackerType = serverTrackerType;
            AchievementDef.SetAchievedIcon(achievementIcon);

            UnlockableFrame<TUnlockableDef> unlockableFrame = new UnlockableFrame<TUnlockableDef>
            {
                name = unlockableRewardName,
                achievementIcon = achievementIcon,
                overrideNameToken = overrideUnlockableNameToken,
                getHowToUnlockString = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                {
                    Language.GetString(AchievementDef.nameToken),
                    Language.GetString(AchievementDef.descriptionToken)
                }),
                getUnlockedString = () => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                {
                    Language.GetString(AchievementDef.nameToken),
                    Language.GetString(AchievementDef.descriptionToken)
                })
            };
            unlockableFrame.BuildInternal(callingMod);
            UnlockableDef = unlockableFrame.UnlockableDef;
        }
    }
    /*public class AchievementFrame<TBaseAchievement, TBaseServerAchievement, TAchievementDef, TUnlockableDef> : AchievementFrame<TAchievementDef, TUnlockableDef>
        where TBaseAchievement : BaseAchievement
        where TBaseServerAchievement : BaseServerAchievement
        where TAchievementDef : AchievementDef
        where TUnlockableDef : UnlockableDef
    {
        [Obsolete(".trackerType was assigned by the generic type.", false)]
        public new Type trackerType
        {
            get => base.trackerType;
            set => base.trackerType = value;
        }
        [Obsolete(".serverTrackerType was assigned by the generic type.", false)]
        public new Type serverTrackerType
        {
            get => base.serverTrackerType;
            set => base.serverTrackerType = value;
        }
        public AchievementFrame()
        {
            base.trackerType = typeof(TBaseAchievement);
            base.serverTrackerType = typeof(TBaseServerAchievement);
        }
    }
    public class AchievementFrame<TBaseAchievement, TAchievementDef, TUnlockableDef> : AchievementFrame<TAchievementDef, TUnlockableDef>
        where TBaseAchievement : BaseAchievement
        where TAchievementDef : AchievementDef
        where TUnlockableDef : UnlockableDef
    {
        [Obsolete(".trackerType was assigned by the generic type.", false)]
        public new Type trackerType
        {
            get => base.trackerType;
            set => base.trackerType = value;
        }
        public AchievementFrame()
        {
            base.trackerType = typeof(TBaseAchievement);
        }
    }*/
}
