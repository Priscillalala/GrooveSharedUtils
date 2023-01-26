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
    public struct ArtifactCodeInfo
    {
        public Vector3Int topRow;
        public Vector3Int middleRow;
        public Vector3Int bottomRow;
        public ArtifactCodeInfo(int topLeft, int topCenter, int topRight, int middleLeft, int middleCenter, int middleRight, int bottomLeft, int bottomCenter, int bottomRight)
        {
            topRow = new Vector3Int(topLeft, topCenter, topRight);
            middleRow = new Vector3Int(middleLeft, middleCenter, middleRight);
            bottomRow = new Vector3Int(bottomLeft, bottomCenter, bottomRight);
        }
        public ArtifactCodeInfo(Vector3Int topRow, Vector3Int middleRow, Vector3Int bottomRow)
        {
            this.topRow = topRow;
            this.middleRow = middleRow;
            this.bottomRow = bottomRow;
        }
        public void SetTopRow(int left, int center, int right)
        {
            topRow = new Vector3Int(left, center, right);
        }
        public void SetMiddleRow(int left, int center, int right)
        {
            middleRow = new Vector3Int(left, center, right);
        }
        public void SetBottomRow(int left, int center, int right)
        {
            bottomRow = new Vector3Int(left, center, right);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public ArtifactCode ToArtifactCode()
        {
            ArtifactCode artifactCode = ScriptableObject.CreateInstance<ArtifactCode>();
            artifactCode.topRow = topRow;
            artifactCode.middleRow = middleRow;
            artifactCode.bottomRow = bottomRow;
            return artifactCode;
        }
    }
}
