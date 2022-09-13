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

namespace GrooveSharedUtils.ScriptableObjects
{
    public abstract class ModdedScriptableObject : ScriptableObject 
	{
		private bool registered;
		public void Register()
		{
            if (!registered)
            {
				RegisterInternal();
				registered = true;
            }
		}
		internal abstract void RegisterInternal();
		private void Awake()
		{
			this._cachedName = base.name;
		}
		private void OnValidate()
		{
			this._cachedName = base.name;
		}
		[Obsolete(".name should not be used. Use .cachedName instead. If retrieving the value from the engine is absolutely necessary, cast to ScriptableObject first.", true)]
		public new string name
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}
		public string cachedName
		{
			get
			{
				return this._cachedName;
			}
			set
			{
				base.name = value;
				this._cachedName = value;
			}
		}
		private string _cachedName;
	}
}
