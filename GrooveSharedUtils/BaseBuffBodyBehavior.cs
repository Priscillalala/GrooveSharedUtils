using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using System.Collections.Generic;
using HG;
using HG.Reflection;
using JetBrains.Annotations;
using System.Reflection;
using BepInEx.Logging;

namespace GrooveSharedUtils
{
	public abstract class BaseBuffBodyBehavior : MonoBehaviour
	{
		public CharacterBody body { get; private set; }

		protected void Awake()
		{
			this.body = earlyAssignmentBody;
			earlyAssignmentBody = null;
		}

		[SystemInitializer(new Type[]
		{
			typeof(BuffCatalog)
		})]
		internal static void Init()
		{
			var server = new Dictionary<BuffIndex, List<BuffBehaviorType>>();
			int serverLength = 0;
			var client = new Dictionary<BuffIndex, List<BuffBehaviorType>>();
			int clientLength = 0;
			var shared = new Dictionary<BuffIndex, List<BuffBehaviorType>>();
			int sharedLength = 0;
			List<BuffDefAssociationAttribute> attributeList = new List<BuffDefAssociationAttribute>();
			HG.Reflection.SearchableAttribute.GetInstances(attributeList);

			Type buffBehaviourType = typeof(BaseBuffBodyBehavior);
			Type buffDefType = typeof(BuffDef);
			foreach (BuffDefAssociationAttribute buffDefAssociationAttribute in attributeList)
			{
				MethodInfo methodInfo;
				if ((methodInfo = (buffDefAssociationAttribute.target as MethodInfo)) == null)
				{
					GroovyLogger.Log(LogLevel.Error, string.Format("{0} cannot be applied to object of type '{1}'", new object[]
					{
						nameof(BuffDefAssociationAttribute),
						buffDefAssociationAttribute?.GetType().FullName
					}));
				}
				else if (!methodInfo.IsStatic)
				{
					GroovyLogger.Log(LogLevel.Error, string.Format("{2} cannot be applied to method {0}.{1}: Method is not static.", new object[]
					{
						methodInfo.DeclaringType.FullName,
						methodInfo.Name,
						nameof(BuffDefAssociationAttribute)
					}));
				}
				else
				{
					Type type = buffDefAssociationAttribute.behaviorTypeOverride ?? methodInfo.DeclaringType;
					if (!buffBehaviourType.IsAssignableFrom(type))
					{
						GroovyLogger.Log(LogLevel.Error, string.Format("{3} cannot be applied to method {0}.{1}: {0} does not derive from {2}.", new object[]
						{
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							buffBehaviourType.FullName,
							nameof(BuffDefAssociationAttribute)
						}));
					}
					else if (type.IsAbstract)
					{
						GroovyLogger.Log(LogLevel.Error, string.Format("{2} cannot be applied to method {0}.{1}: {0} is an abstract type", new object[]
						{
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							nameof(BuffDefAssociationAttribute)
						}));
					}
					else if (!buffDefType.IsAssignableFrom(methodInfo.ReturnType))
					{
						Type returnType = methodInfo.ReturnType;
						GroovyLogger.Log(LogLevel.Error, string.Format("{0} cannot be applied to method {1}.{2}: {3}.{4} returns type '{5}' instead of {6}.", new object[] 
						{
							nameof(BuffDefAssociationAttribute),
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							methodInfo.DeclaringType.FullName,
							methodInfo,
							(returnType?.FullName) ?? "void",
							buffDefType.FullName
					}));
					}
					else if (methodInfo.GetGenericArguments().Length != 0)
					{
						GroovyLogger.Log(LogLevel.Error, string.Format("{0} cannot be applied to method {1}.{2}: {3}.{4} must take no arguments.", new object[]
						{
							nameof(BuffDefAssociationAttribute),
							methodInfo.DeclaringType.FullName,
							methodInfo.Name,
							methodInfo.DeclaringType.FullName,
							methodInfo
						}));
					}
					else
					{
						BuffDef buffDef = (BuffDef)methodInfo.Invoke(null, Array.Empty<object>());
						if (!buffDef)
						{
							GroovyLogger.Log(LogLevel.Error, methodInfo.DeclaringType.FullName + "." + methodInfo.Name + " returned null.");
						}
						else if (buffDef.buffIndex < 0)
						{
							GroovyLogger.Log(LogLevel.Error, string.Format("{0}.{1} returned a BuffDef that's not registered in the BuffCatalog. result={2}", 
								methodInfo.DeclaringType.FullName, 
								methodInfo.Name, 
								buffDef));
						}
						else
						{
							if (buffDefAssociationAttribute.useOnServer)
							{
								server.GetOrCreateValue(buffDef.buffIndex).Add(new BuffBehaviorType(type, serverLength++));
							}
							if (buffDefAssociationAttribute.useOnClient)
							{
								client.GetOrCreateValue(buffDef.buffIndex).Add(new BuffBehaviorType(type, clientLength++));
							}
							if (buffDefAssociationAttribute.useOnServer || buffDefAssociationAttribute.useOnClient)
							{
								shared.GetOrCreateValue(buffDef.buffIndex).Add(new BuffBehaviorType(type, sharedLength++));
							}
						}
					}
				}
			}
			BaseBuffBodyBehavior.server.SetBuffsToBehaviors(server, serverLength);
			BaseBuffBodyBehavior.client.SetBuffsToBehaviors(client, clientLength);
			BaseBuffBodyBehavior.shared.SetBuffsToBehaviors(shared, sharedLength);
			if(sharedLength <= 0)
            {
				return;
            }
			CharacterBody.onBodyAwakeGlobal += OnBodyAwakeGlobal;
			CharacterBody.onBodyDestroyGlobal += OnBodyDestroyGlobal;
			On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;
		}
		private static void OnBodyAwakeGlobal(CharacterBody body)
		{
			BaseBuffBodyBehavior[] value = GetNetworkContext().behaviorArraysPool.Request();
			bodyToBuffBehaviors.Add(body, value);
		}

		private static void OnBodyDestroyGlobal(CharacterBody body)
		{
			BaseBuffBodyBehavior[] array = bodyToBuffBehaviors[body];
			for (int i = 0; i < array.Length; i++)
			{
				Destroy(array[i]);
			}
			bodyToBuffBehaviors.Remove(body);
			if (NetworkServer.active || NetworkClient.active)
			{
				GetNetworkContext().behaviorArraysPool.Return(array);
			}
		}

		private static void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
			orig(self, buffType, newCount);
			ref NetworkContextSet networkContext = ref GetNetworkContext();
			if (networkContext.buffsToBehaviors.TryGetValue(buffType, out List<BuffBehaviorType> list))
			{
				BaseBuffBodyBehavior[] array = bodyToBuffBehaviors[self];
				int stack = self.GetBuffCount(buffType);
				foreach(BuffBehaviorType buffBehavior in list)
                {
					SetBuffStack(self, ref array[buffBehavior.behaviorArrayIndex], buffBehavior.behaviorType, stack);
                }
			}

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

		private static void SetBuffStack(CharacterBody body, ref BaseBuffBodyBehavior behavior, Type behaviorType, int stack)
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
					earlyAssignmentBody = body;
					behavior = (BaseBuffBodyBehavior)body.gameObject.AddComponent(behaviorType);
					earlyAssignmentBody = null;
				}
			}
			if (behavior != null)
			{
				behavior.stack = stack;
			}
		}

		public int stack;

		private static NetworkContextSet server;

		private static NetworkContextSet client;

		private static NetworkContextSet shared;

		private static CharacterBody earlyAssignmentBody = null;

		private static Dictionary<UnityObjectWrapperKey<CharacterBody>, BaseBuffBodyBehavior[]> bodyToBuffBehaviors = new Dictionary<UnityObjectWrapperKey<CharacterBody>, BaseBuffBodyBehavior[]>();
		private struct BuffBehaviorType
		{
			public BuffBehaviorType(Type type, int index)
            {
				behaviorType = type;
				behaviorArrayIndex = index;
            }
			public Type behaviorType;
			public int behaviorArrayIndex;
		}
		private struct NetworkContextSet
		{
			public void SetBuffsToBehaviors(Dictionary<BuffIndex, List<BuffBehaviorType>> buffsToBehaviors, int behaviorArrayLength)
			{
				this.buffsToBehaviors = buffsToBehaviors;
				this.behaviorArraysPool = new FixedSizeArrayPool<BaseBuffBodyBehavior>(behaviorArrayLength);
			}

			public Dictionary<BuffIndex, List<BuffBehaviorType>> buffsToBehaviors;

			public FixedSizeArrayPool<BaseBuffBodyBehavior> behaviorArraysPool;
		}

		[MeansImplicitUse]
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		public class BuffDefAssociationAttribute : HG.Reflection.SearchableAttribute
		{
			public Type behaviorTypeOverride;

			public bool useOnServer = true;

			public bool useOnClient = true;
		}
	}
}
