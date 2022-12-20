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
    [CreateAssetMenu(menuName = "GrooveSharedUtils/ModdedDotDef")]
    public class ModdedDotDef : ModdedScriptableObject
    {
        public DotController.DotIndex dotIndex { get; set; }
		public float interval;
		public float damageCoefficient;
		public DamageColorIndex damageColorIndex;
		public ModdedDamageColorDef moddedDamageColor = null;
		public BuffDef associatedBuff;
		public BuffDef terminalTimedBuff;
		public float terminalTimedBuffDuration;
		public bool resetTimerOnAdd;
		public DotAPI.CustomDotBehaviour customDotBehaviour = null;
		public DotAPI.CustomDotVisual customDotVisual = null;

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		protected override void RegisterInternal()
        {
			DotController.DotDef dotDef = new DotController.DotDef
			{
				interval = interval,
				damageCoefficient = damageCoefficient,
				damageColorIndex = moddedDamageColor ? moddedDamageColor.damageColorIndex : damageColorIndex,
				associatedBuff = associatedBuff,
				terminalTimedBuff = terminalTimedBuff,
				terminalTimedBuffDuration = terminalTimedBuffDuration,
				resetTimerOnAdd = resetTimerOnAdd
			};
			dotIndex = DotAPI.RegisterDotDef(dotDef, customDotBehaviour, customDotVisual);
		}
	}
}
