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
using R2API.ScriptableObjects;
using GrooveSharedUtils.Frames;
using UnityEngine.AddressableAssets;
using System.Linq;
using R2API;
using System.Runtime.CompilerServices;
using System.Collections;
using RoR2.ContentManagement;
using UnityEngine.Events;
using BepInEx.Configuration;

namespace GrooveSharedUtils
{
    public struct ModelPanelParametersInfo
    {
        public Quaternion modelRotation;
        public float minDistance;
        public float maxDistance;
        public Transform focusPoint;
        public Transform cameraPosition;
        public ModelPanelParametersInfo(Vector3 modelRotation, float minDistance, float maxDistance, Transform focusPoint = null, Transform cameraPosition = null)
            : this(Quaternion.Euler(modelRotation), minDistance, maxDistance, focusPoint, cameraPosition) { }
        public ModelPanelParametersInfo(Quaternion modelRotation, float minDistance, float maxDistance, Transform focusPoint = null, Transform cameraPosition = null)
        {
            this.modelRotation = modelRotation;
            this.minDistance = minDistance;
            this.maxDistance = maxDistance;
            this.focusPoint = focusPoint;
            this.cameraPosition = cameraPosition;
        }
    }
}
