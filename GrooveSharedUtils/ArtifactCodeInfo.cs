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
using RoR2;
using ThreeEyedGames;

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
        public void CopyToFormulaDisplay(ArtifactFormulaDisplay formulaDisplay)
        {
            formulaDisplay.artifactCompoundDisplayInfos = new[]
            {
                GetDisplayInfo(formulaDisplay, topRow.x, "Slot 1,1"),
                GetDisplayInfo(formulaDisplay, topRow.y, "Slot 1,2"),
                GetDisplayInfo(formulaDisplay, topRow.z, "Slot 1,3"),
                GetDisplayInfo(formulaDisplay, middleRow.x, "Slot 2,1"),
                GetDisplayInfo(formulaDisplay, middleRow.y, "Slot 2,2"),
                GetDisplayInfo(formulaDisplay, middleRow.z, "Slot 2,3"),
                GetDisplayInfo(formulaDisplay, bottomRow.x, "Slot 3,1"),
                GetDisplayInfo(formulaDisplay, bottomRow.y, "Slot 3,2"),
                GetDisplayInfo(formulaDisplay, bottomRow.z, "Slot 3,3"),
            };
        }
        private ArtifactFormulaDisplay.ArtifactCompoundDisplayInfo GetDisplayInfo(ArtifactFormulaDisplay formulaDisplay, int value, string decalPath)
        {
            return new ArtifactFormulaDisplay.ArtifactCompoundDisplayInfo
            {
                artifactCompoundDef = GSUtil.FindArtifactCompoundDef(value),
                decal = formulaDisplay.transform.Find(decalPath)?.GetComponent<Decal>()
            };
        }
    }
}
