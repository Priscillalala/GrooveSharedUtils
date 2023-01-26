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
using System.Runtime.CompilerServices;

namespace GrooveSharedUtils.ScriptableObjects
{
    [CreateAssetMenu(menuName = "GrooveSharedUtils/ModdedDifficultyDef")]
    public class ModdedDifficultyDef : ModdedScriptableObject
    {
        public DifficultyIndex difficultyIndex { get; set; }
		public DifficultyDef difficultyDef { get; set; }
		public float scalingValue;
		public string nameToken;
		public string descriptionToken;
		public Sprite icon;
		public Color color;
		public string serverTag;
		public bool countsAsHardMode;

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		protected override void RegisterInternal()
        {
			difficultyDef = new DifficultyDef(scalingValue, nameToken, string.Empty, descriptionToken, color, serverTag, countsAsHardMode);
			difficultyDef.foundIconSprite = true;
			difficultyDef.iconSprite = icon;
			difficultyIndex = DifficultyAPI.AddDifficulty(difficultyDef);
		}
	}
}
