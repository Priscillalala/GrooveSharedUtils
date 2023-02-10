using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using RoR2.Items;
using System.Collections.Generic;
using HG;
using HG.Reflection;
using JetBrains.Annotations;
using System.Reflection;
using BepInEx.Logging;

namespace GrooveSharedUtils
{
	public abstract class BaseItemMasterBehavior : MonoBehaviour
	{
		public CharacterMaster master { get; private set; }

		protected void Awake()
		{
			this.master = BaseItemMasterBehavior.earlyAssignmentMaster;
			BaseItemMasterBehavior.earlyAssignmentMaster = null;
		}

		[SystemInitializer(new Type[]
		{
			typeof(ItemCatalog)
		})]
		internal static void Init()
		{
			List<BaseItemBodyBehavior.ItemTypePair> server = new List<BaseItemBodyBehavior.ItemTypePair>();
			List<BaseItemBodyBehavior.ItemTypePair> client = new List<BaseItemBodyBehavior.ItemTypePair>();
			List<BaseItemBodyBehavior.ItemTypePair> shared = new List<BaseItemBodyBehavior.ItemTypePair>();
			List<ItemDefAssociationAttribute> attributeList = new List<ItemDefAssociationAttribute>();
			HG.Reflection.SearchableAttribute.GetInstances(attributeList);

			Type masterBehaviourType = typeof(BaseItemMasterBehavior);
			Type itemDefType = typeof(ItemDef);
			foreach (ItemDefAssociationAttribute itemDefAssociationAttribute in attributeList)
			{
				MethodInfo methodInfo;
				if ((methodInfo = (itemDefAssociationAttribute.target as MethodInfo)) == null)
				{
					GroovyLogger.Log(LogLevel.Error, string.Format("{0} cannot be applied to object of type '{1}'", new object[]
					{
						nameof(ItemDefAssociationAttribute),
						itemDefAssociationAttribute?.GetType().FullName
					}));
				}
				else if (!methodInfo.IsStatic)
				{
					GroovyLogger.Log(LogLevel.Error, string.Format("{2} cannot be applied to method {0}.{1}: Method is not static.", new object[]
					{
						methodInfo.DeclaringType.FullName,
						methodInfo.Name,
						nameof(ItemDefAssociationAttribute)
					}));
				}
				else
				{
					Type type = itemDefAssociationAttribute.behaviorTypeOverride ?? methodInfo.DeclaringType;
					if (!masterBehaviourType.IsAssignableFrom(type))
					{
						GroovyLogger.Log(LogLevel.Error, string.Format("{3} cannot be applied to method {0}.{1}: {0} does not derive from {2}.", new object[]
						{
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							masterBehaviourType.FullName,
							nameof(ItemDefAssociationAttribute)
						}));
					}
					else if (type.IsAbstract)
					{
						GroovyLogger.Log(LogLevel.Error, string.Format("{2} cannot be applied to method {0}.{1}: {0} is an abstract type", new object[]
						{
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							nameof(ItemDefAssociationAttribute)
						}));
					}
					else if (!itemDefType.IsAssignableFrom(methodInfo.ReturnType))
					{
						Type returnType = methodInfo.ReturnType;
						GroovyLogger.Log(LogLevel.Error, string.Format("{0} cannot be applied to method {1}.{2}: {3}.{4} returns type '{5}' instead of {6}.", new object[] 
						{
							nameof(ItemDefAssociationAttribute),
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							methodInfo.DeclaringType.FullName,
							methodInfo,
							(returnType?.FullName) ?? "void",
							itemDefType.FullName
					}));
					}
					else if (methodInfo.GetGenericArguments().Length != 0)
					{
						GroovyLogger.Log(LogLevel.Error, string.Format("{0} cannot be applied to method {1}.{2}: {3}.{4} must take no arguments.", new object[]
						{
							nameof(ItemDefAssociationAttribute),
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							methodInfo.DeclaringType.FullName,
							methodInfo
						}));
					}
					else
					{
						ItemDef itemDef = (ItemDef)methodInfo.Invoke(null, Array.Empty<object>());
						if (!itemDef)
						{
							GroovyLogger.Log(LogLevel.Error, methodInfo.DeclaringType.FullName + "." + methodInfo.Name + " returned null.");
						}
						else if (itemDef.itemIndex < 0)
						{
							GroovyLogger.Log(LogLevel.Error, string.Format("{0}.{1} returned an ItemDef that's not registered in the ItemCatalog. result={2}", 
								methodInfo.DeclaringType.FullName, 
								methodInfo.Name, 
								itemDef));
						}
						else
						{
							if (itemDefAssociationAttribute.useOnServer)
							{
								server.Add(new BaseItemBodyBehavior.ItemTypePair
								{
									itemIndex = itemDef.itemIndex,
									behaviorType = type
								});
							}
							if (itemDefAssociationAttribute.useOnClient)
							{
								client.Add(new BaseItemBodyBehavior.ItemTypePair
								{
									itemIndex = itemDef.itemIndex,
									behaviorType = type
								});
							}
							if (itemDefAssociationAttribute.useOnServer || itemDefAssociationAttribute.useOnClient)
							{
								shared.Add(new BaseItemBodyBehavior.ItemTypePair
								{
									itemIndex = itemDef.itemIndex,
									behaviorType = type
								});
							}
						}
					}
				}
			}
			BaseItemMasterBehavior.server.SetItemTypePairs(server);
			BaseItemMasterBehavior.client.SetItemTypePairs(client);
			BaseItemMasterBehavior.shared.SetItemTypePairs(shared);
			if (BaseItemMasterBehavior.shared.itemTypePairs.Length <= 0)
			{
				return;
			}
			On.RoR2.CharacterMaster.Awake += CharacterMaster_Awake;
			On.RoR2.CharacterMaster.OnDestroy += CharacterMaster_OnDestroy;
			On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
		}

		private static void CharacterMaster_Awake(On.RoR2.CharacterMaster.orig_Awake orig, CharacterMaster self)
		{
			BaseItemMasterBehavior[] value = BaseItemMasterBehavior.GetNetworkContext().behaviorArraysPool.Request();
			BaseItemMasterBehavior.masterToItemBehaviors.Add(self, value);
			orig(self);
		}

		private static void CharacterMaster_OnDestroy(On.RoR2.CharacterMaster.orig_OnDestroy orig, CharacterMaster self)
		{
			orig(self);
			BaseItemMasterBehavior[] array = BaseItemMasterBehavior.masterToItemBehaviors[self];
			for (int i = 0; i < array.Length; i++)
			{
				Destroy(array[i]);
			}
			BaseItemMasterBehavior.masterToItemBehaviors.Remove(self);
			if (NetworkServer.active || NetworkClient.active)
			{
				BaseItemMasterBehavior.GetNetworkContext().behaviorArraysPool.Return(array);
			}
		}

		private static void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
		{
			orig(self);
			BaseItemMasterBehavior.UpdateMasterItemBehaviorStacks(self);

		}

		private static ref BaseItemMasterBehavior.NetworkContextSet GetNetworkContext()
		{
			bool networkActive = NetworkServer.active;
			bool clientActive = NetworkClient.active;
			if (networkActive)
			{
				if (clientActive)
				{
					return ref BaseItemMasterBehavior.shared;
				}
				return ref BaseItemMasterBehavior.server;
			}
			else
			{
				if (clientActive)
				{
					return ref BaseItemMasterBehavior.client;
				}
				throw new InvalidOperationException("Neither server nor client is running.");
			}
		}

		private static void UpdateMasterItemBehaviorStacks(CharacterMaster master)
		{
			ref BaseItemMasterBehavior.NetworkContextSet networkContext = ref BaseItemMasterBehavior.GetNetworkContext();
			BaseItemMasterBehavior[] array = BaseItemMasterBehavior.masterToItemBehaviors[master];
			BaseItemBodyBehavior.ItemTypePair[] itemTypePairs = networkContext.itemTypePairs;
			Inventory inventory = master.inventory;
			if (inventory)
			{
				for (int i = 0; i < itemTypePairs.Length; i++)
				{
					BaseItemBodyBehavior.ItemTypePair itemTypePair = itemTypePairs[i];
					ref BaseItemMasterBehavior behavior = ref array[i];
					BaseItemMasterBehavior.SetItemStack(master, ref behavior, itemTypePair.behaviorType, inventory.GetItemCount(itemTypePair.itemIndex));
				}
				return;
			}
			for (int j = 0; j < itemTypePairs.Length; j++)
			{
				ref BaseItemMasterBehavior ptr = ref array[j];
				if (ptr != null)
				{
					Destroy(ptr);
					ptr = null;
				}
			}
		}

		private static void SetItemStack(CharacterMaster master, ref BaseItemMasterBehavior behavior, Type behaviorType, int stack)
		{
			if (behavior == null != stack <= 0)
			{
				if (stack <= 0)
				{
					behavior.stack = 0;
					Destroy(behavior);
					behavior = null;
				}
				else
				{
					BaseItemMasterBehavior.earlyAssignmentMaster = master;
					behavior = (BaseItemMasterBehavior)master.gameObject.AddComponent(behaviorType);
					BaseItemMasterBehavior.earlyAssignmentMaster = null;
				}
			}
			if (behavior != null)
			{
				behavior.stack = stack;
			}
		}

		public int stack;

		private static BaseItemMasterBehavior.NetworkContextSet server;

		private static BaseItemMasterBehavior.NetworkContextSet client;

		private static BaseItemMasterBehavior.NetworkContextSet shared;

		private static CharacterMaster earlyAssignmentMaster = null;

		private static Dictionary<UnityObjectWrapperKey<CharacterMaster>, BaseItemMasterBehavior[]> masterToItemBehaviors = new Dictionary<UnityObjectWrapperKey<CharacterMaster>, BaseItemMasterBehavior[]>();

		private struct NetworkContextSet
		{
			public void SetItemTypePairs(List<BaseItemBodyBehavior.ItemTypePair> itemTypePairs)
			{
				this.itemTypePairs = itemTypePairs.ToArray();
				this.behaviorArraysPool = new FixedSizeArrayPool<BaseItemMasterBehavior>(this.itemTypePairs.Length);
			}

			public BaseItemBodyBehavior.ItemTypePair[] itemTypePairs;

			public FixedSizeArrayPool<BaseItemMasterBehavior> behaviorArraysPool;
		}

		[MeansImplicitUse]
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		public class ItemDefAssociationAttribute : HG.Reflection.SearchableAttribute
		{
			public Type behaviorTypeOverride;

			public bool useOnServer = true;

			public bool useOnClient = true;
		}
	}
}
