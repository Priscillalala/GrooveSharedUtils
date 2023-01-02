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
using System.Linq;

namespace GrooveSharedUtils
{
	public abstract class BaseEquipmentMasterBehavior : MonoBehaviour
	{
		public CharacterMaster master { get; private set; }

		protected void Awake()
		{
			this.master = earlyAssignmentMaster;
			earlyAssignmentMaster = null;
		}

		[SystemInitializer(new Type[]
		{
			typeof(ItemCatalog)
		})]
		internal static void Init()
		{
			List<BaseEquipmentMasterBehavior.EquipmentTypePair> server = new List<BaseEquipmentMasterBehavior.EquipmentTypePair>();
			List<BaseEquipmentMasterBehavior.EquipmentTypePair> client = new List<BaseEquipmentMasterBehavior.EquipmentTypePair>();
			List<BaseEquipmentMasterBehavior.EquipmentTypePair> shared = new List<BaseEquipmentMasterBehavior.EquipmentTypePair>();
			List<EquipmentDefAssociationAttribute> attributeList = new List<EquipmentDefAssociationAttribute>();
			HG.Reflection.SearchableAttribute.GetInstances(attributeList);

			Type masterBehaviourType = typeof(BaseItemMasterBehavior);
			Type itemDefType = typeof(ItemDef);
			foreach (EquipmentDefAssociationAttribute itemDefAssociationAttribute in attributeList)
			{
				MethodInfo methodInfo;
				if ((methodInfo = (itemDefAssociationAttribute.target as MethodInfo)) == null)
				{
					GSUtil.Log(LogLevel.Error, string.Format("{0} cannot be applied to object of type '{1}'", new object[]
					{
						nameof(EquipmentDefAssociationAttribute),
						itemDefAssociationAttribute?.GetType().FullName
					}));
				}
				else if (!methodInfo.IsStatic)
				{
					GSUtil.Log(LogLevel.Error, string.Format("{2} cannot be applied to method {0}.{1}: Method is not static.", new object[]
					{
						methodInfo.DeclaringType.FullName,
						methodInfo.Name,
						nameof(EquipmentDefAssociationAttribute)
					}));
				}
				else
				{
					Type type = itemDefAssociationAttribute.behaviorTypeOverride ?? methodInfo.DeclaringType;
					if (!masterBehaviourType.IsAssignableFrom(type))
					{
						GSUtil.Log(LogLevel.Error, string.Format("{3} cannot be applied to method {0}.{1}: {0} does not derive from {2}.", new object[]
						{
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							masterBehaviourType.FullName,
							nameof(EquipmentDefAssociationAttribute)
						}));
					}
					else if (type.IsAbstract)
					{
						GSUtil.Log(LogLevel.Error, string.Format("{2} cannot be applied to method {0}.{1}: {0} is an abstract type", new object[]
						{
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							nameof(EquipmentDefAssociationAttribute)
						}));
					}
					else if (!itemDefType.IsAssignableFrom(methodInfo.ReturnType))
					{
						Type returnType = methodInfo.ReturnType;
						GSUtil.Log(LogLevel.Error, string.Format("{0} cannot be applied to method {1}.{2}: {3}.{4} returns type '{5}' instead of {6}.", new object[] 
						{
							nameof(EquipmentDefAssociationAttribute),
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
						GSUtil.Log(LogLevel.Error, string.Format("{0} cannot be applied to method {1}.{2}: {3}.{4} must take no arguments.", new object[]
						{
							nameof(EquipmentDefAssociationAttribute),
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							methodInfo.DeclaringType.FullName,
							methodInfo
						}));
					}
					else
					{
						EquipmentDef equipmentDef = (EquipmentDef)methodInfo.Invoke(null, Array.Empty<object>());
						if (!equipmentDef)
						{
							GSUtil.Log(LogLevel.Error, methodInfo.DeclaringType.FullName + "." + methodInfo.Name + " returned null.");
						}
						else if (equipmentDef.equipmentIndex < 0)
						{
							GSUtil.Log(LogLevel.Error, string.Format("{0}.{1} returned an EquipmentDef that's not registered in the EquipmentCatalog. result={2}", 
								methodInfo.DeclaringType.FullName, 
								methodInfo.Name,
								equipmentDef));
						}
						else
						{
							EquipmentTypePair equipmentTypePair = new BaseEquipmentMasterBehavior.EquipmentTypePair
							{
								equipmentIndex = equipmentDef.equipmentIndex,
								behaviorType = type,
								requiresActiveSlot = itemDefAssociationAttribute.requiresActiveSlot
							};
							if (itemDefAssociationAttribute.useOnServer)
							{
								server.Add(equipmentTypePair);
							}
							if (itemDefAssociationAttribute.useOnClient)
							{
								client.Add(equipmentTypePair);
							}
							if (itemDefAssociationAttribute.useOnServer || itemDefAssociationAttribute.useOnClient)
							{
								shared.Add(equipmentTypePair);
							}
						}
					}
				}
			}
			BaseEquipmentMasterBehavior.server.SetItemTypePairs(server);
			BaseEquipmentMasterBehavior.client.SetItemTypePairs(client);
			BaseEquipmentMasterBehavior.shared.SetItemTypePairs(shared);
			On.RoR2.CharacterMaster.Awake += CharacterMaster_Awake;
			On.RoR2.CharacterMaster.OnDestroy += CharacterMaster_OnDestroy;
			On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
		}

		private static void CharacterMaster_Awake(On.RoR2.CharacterMaster.orig_Awake orig, CharacterMaster self)
		{
			BaseEquipmentMasterBehavior[] value = GetNetworkContext().behaviorArraysPool.Request();
			masterToItemBehaviors.Add(self, value);
			orig(self);
		}

		private static void CharacterMaster_OnDestroy(On.RoR2.CharacterMaster.orig_OnDestroy orig, CharacterMaster self)
		{
			orig(self);
			BaseEquipmentMasterBehavior[] array = masterToItemBehaviors[self];
			for (int i = 0; i < array.Length; i++)
			{
				Destroy(array[i]);
			}
			masterToItemBehaviors.Remove(self);
			if (NetworkServer.active || NetworkClient.active)
			{
				GetNetworkContext().behaviorArraysPool.Return(array);
			}
		}

		private static void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
		{
			orig(self);
			UpdateMasterEquipmentBehaviors(self);

		}

		private static ref NetworkContextSet GetNetworkContext()
		{
			bool networkActive = NetworkServer.active;
			bool clientActive = NetworkClient.active;
			if (networkActive)
			{
				if (clientActive)
				{
					return ref shared;
				}
				return ref server;
			}
			else
			{
				if (clientActive)
				{
					return ref client;
				}
				throw new InvalidOperationException("Neither server nor client is running.");
			}
		}

		private static void UpdateMasterEquipmentBehaviors(CharacterMaster master)
		{
			ref NetworkContextSet networkContext = ref GetNetworkContext();
			BaseEquipmentMasterBehavior[] array = masterToItemBehaviors[master];
			BaseEquipmentMasterBehavior.EquipmentTypePair[] itemTypePairs = networkContext.itemTypePairs;
			Inventory inventory = master.inventory;
			if (inventory)
			{
				for (int i = 0; i < itemTypePairs.Length; i++)
				{
					BaseEquipmentMasterBehavior.EquipmentTypePair itemTypePair = itemTypePairs[i];
					ref BaseEquipmentMasterBehavior behavior = ref array[i];
					currentHeldEquipmentIndices.UnionWith(from state in inventory.equipmentStateSlots select state.equipmentIndex);
					HandleEquipmentTypePair(master, ref behavior, itemTypePair, inventory.currentEquipmentIndex, currentHeldEquipmentIndices);
					currentHeldEquipmentIndices.Clear();
				}
				return;
			}
			for (int j = 0; j < itemTypePairs.Length; j++)
			{
				ref BaseEquipmentMasterBehavior ptr = ref array[j];
				if (ptr != null)
				{
					Destroy(ptr);
					ptr = null;
				}
			}
		}

		private static void HandleEquipmentTypePair(CharacterMaster master, ref BaseEquipmentMasterBehavior behavior, EquipmentTypePair pair, EquipmentIndex activeEquipmentIndex, HashSet<EquipmentIndex> heldEquipmentIndices)
		{
			bool shouldHaveBehavior = pair.requiresActiveSlot ? activeEquipmentIndex == pair.equipmentIndex : heldEquipmentIndices.Contains(pair.equipmentIndex);
			if (behavior != shouldHaveBehavior)
			{
				if (shouldHaveBehavior)
				{
					earlyAssignmentMaster = master;
					behavior = (BaseEquipmentMasterBehavior)master.gameObject.AddComponent(pair.behaviorType);
					earlyAssignmentMaster = null;

				}
				else
				{
					behavior.isActiveSlot = false;
					Destroy(behavior);
					behavior = null;
				}
			}
			if (behavior != null)
			{
				behavior.isActiveSlot = activeEquipmentIndex == pair.equipmentIndex;
			}
		}

		public bool isActiveSlot;

		private static NetworkContextSet server;

		private static NetworkContextSet client;

		private static NetworkContextSet shared;

		private static CharacterMaster earlyAssignmentMaster = null;

		private static readonly HashSet<EquipmentIndex> currentHeldEquipmentIndices = new HashSet<EquipmentIndex>();

		private static Dictionary<UnityObjectWrapperKey<CharacterMaster>, BaseEquipmentMasterBehavior[]> masterToItemBehaviors = new Dictionary<UnityObjectWrapperKey<CharacterMaster>, BaseEquipmentMasterBehavior[]>();

		public struct EquipmentTypePair
        {
			public EquipmentIndex equipmentIndex;
			public Type behaviorType;
			public bool requiresActiveSlot;
        }
		private struct NetworkContextSet
		{
			public void SetItemTypePairs(List<BaseEquipmentMasterBehavior.EquipmentTypePair> itemTypePairs)
			{
				this.itemTypePairs = itemTypePairs.ToArray();
				this.behaviorArraysPool = new FixedSizeArrayPool<BaseEquipmentMasterBehavior>(this.itemTypePairs.Length);
			}

			public EquipmentTypePair[] itemTypePairs;

			public FixedSizeArrayPool<BaseEquipmentMasterBehavior> behaviorArraysPool;
		}

		[MeansImplicitUse]
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		public class EquipmentDefAssociationAttribute : HG.Reflection.SearchableAttribute
		{
			public Type behaviorTypeOverride;

			public bool useOnServer = true;

			public bool useOnClient = true;

			public bool requiresActiveSlot = false;
		}
	}
}
