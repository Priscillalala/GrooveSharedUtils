using System;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

namespace GrooveSharedUtils.MSUMaterialControllerComponents
{
    /* Copyright © 2022 TeamMoonstorm

Permission is hereby granted, free of charge, to any person depending on this software and associated documentation files (the "Software") to deal in the software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense.

Other software can reuse portions of the code as long as this License alongside a "Thanks" message is included on the using software's readme

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the software

All rights are reserved. */
    public class HGOpaqueCloudRemap : MaterialController
    {

        public Color _TintColor;
        public Color _EmissionColor;
        public Texture _MainTex;
        public Vector2 _MainTexScale;
        public Vector2 _MainTexOffset;
        [Range(0f, 5f)]
        public float _NormalStrength;
        public Texture _NormalTex;
        public Texture _Cloud1Tex;
        public Vector2 _Cloud1TexScale;
        public Vector2 _Cloud1TexOffset;
        public Texture _Cloud2Tex;
        public Vector2 _Cloud2TexScale;
        public Vector2 _Cloud2TexOffset;
        public Texture _RemapTex;
        public Vector2 _RemapTexScale;
        public Vector2 _RemapTexOffset;
        public Vector4 _CutoffScroll;
        [Range(0f, 30f)]
        public float _InvFade;
        [Range(0f, 20f)]
        public float _AlphaBoost;
        [Range(0f, 1f)]
        public float _Cutoff;
        [Range(0f, 1f)]
        public float _SpecularStrength;
        [Range(0.1f, 20f)]
        public float _SpecularExponent;
        [Range(0f, 10f)]
        public float _ExtrusionStrength;
        public enum _RampEnum
        {
            TwoTone = 0,
            SmoothedTwoTone = 1,
            Unlitish = 3,
            Subsurface = 4,
            Grass = 5
        }
        public _RampEnum _RampInfo;
        public bool _EmissionFromAlbedo;
        public bool _CloudNormalMap;
        public bool _VertexAlphaOn;
        public enum _CullEnum
        {
            Off = 0,
            Front = 1,
            Back = 2
        }
        public _CullEnum _Cull;
        public float _ExternalAlpha;

        public override void GrabMaterialValues()
        {
            if (material)
            {
                _TintColor = material.GetColor("_TintColor");
                _EmissionColor = material.GetColor("_EmissionColor");
                _MainTex = material.GetTexture("_MainTex");
                _MainTexScale = material.GetTextureScale("_MainTex");
                _MainTexOffset = material.GetTextureOffset("_MainTex");
                _NormalStrength = material.GetFloat("_NormalStrength");
                _NormalTex = material.GetTexture("_NormalTex");
                _Cloud1Tex = material.GetTexture("_Cloud1Tex");
                _Cloud1TexScale = material.GetTextureScale("_Cloud1Tex");
                _Cloud1TexOffset = material.GetTextureOffset("_Cloud1Tex");
                _Cloud2Tex = material.GetTexture("_Cloud2Tex");
                _Cloud2TexScale = material.GetTextureScale("_Cloud2Tex");
                _Cloud2TexOffset = material.GetTextureScale("_Cloud2Tex");
                _RemapTex = material.GetTexture("_RemapTex");
                _RemapTexScale = material.GetTextureScale("_RemapTex");
                _RemapTexOffset = material.GetTextureOffset("_RemapTex");
                _CutoffScroll = material.GetVector("_CutoffScroll");
                _InvFade = material.GetFloat("_InvFade");
                _AlphaBoost = material.GetFloat("_AlphaBoost");
                _Cutoff = material.GetFloat("_Cutoff");
                _SpecularStrength = material.GetFloat("_SpecularStrength");
                _SpecularExponent = material.GetFloat("_SpecularExponent");
                _ExtrusionStrength = material.GetFloat("_ExtrusionFloat");
                _RampInfo = (_RampEnum)material.GetInt("_RampInfo");
                _EmissionFromAlbedo = material.IsKeywordEnabled("EMISSIONFROMALBEDO");
                _CloudNormalMap = material.IsKeywordEnabled("CLOUDNORMAL");
                _VertexAlphaOn = material.IsKeywordEnabled("VERTEXALPHA");
                _Cull = (_CullEnum)material.GetInt("_Cull");
                _ExternalAlpha = material.GetFloat("_ExternalAlpha");
            }
        }

        public void Update()
        {
            if (material)
            {

                material.SetColor("_TintColor", _TintColor);
                material.SetColor("_EmissionColor", _EmissionColor);

                if (_MainTex)
                {
                    material.SetTexture("_MainTex", _MainTex);
                    material.SetTextureScale("_MainTex", _MainTexScale);
                    material.SetTextureOffset("_MainTex", _MainTexOffset);
                }
                else
                {
                    material.SetTexture("_MainTex", null);
                }

                material.SetFloat("_NormalStrength", _NormalStrength);

                if (_NormalTex)
                {
                    material.SetTexture("_NormalTex", _NormalTex);
                }
                else
                {
                    material.SetTexture("_NormalTex", null);
                }

                if (_Cloud1Tex)
                {
                    material.SetTexture("_Cloud1Tex", _Cloud1Tex);
                    material.SetTextureScale("_Cloud1Tex", _Cloud1TexScale);
                    material.SetTextureOffset("_Cloud1Tex", _Cloud1TexOffset);
                }
                else
                {
                    material.SetTexture("_Cloud1Tex", null);
                }

                if (_Cloud2Tex)
                {
                    material.SetTexture("_Cloud2Tex", _Cloud2Tex);
                    material.SetTextureScale("_Cloud2Tex", _Cloud2TexScale);
                    material.SetTextureOffset("_Cloud2Tex", _Cloud2TexOffset);
                }
                else
                {
                    material.SetTexture("_Cloud2Tex", null);
                }

                if (_RemapTex)
                {
                    material.SetTexture("_RemapTex", _RemapTex);
                    material.SetTextureScale("_RemapTex", _RemapTexScale);
                    material.SetTextureOffset("_RemapTex", _RemapTexOffset);
                }
                else
                {
                    material.SetTexture("_RemapTex", null);
                }

                material.SetVector("_CutoffScroll", _CutoffScroll);
                material.SetFloat("_InvFade", _InvFade);
                material.SetFloat("_AlphaBoost", _AlphaBoost);
                material.SetFloat("_Cutoff", _Cutoff);
                material.SetFloat("_SpecularStrength", _SpecularStrength);
                material.SetFloat("_SpecularExponent", _SpecularExponent);
                material.SetFloat("_ExtrusionStrength", _ExtrusionStrength);
                material.SetInt("_RampInfo", (int)_RampInfo);
                SetShaderKeywordBasedOnBool(_EmissionFromAlbedo, material, "EMISSIONFROMALBEDO");
                SetShaderKeywordBasedOnBool(_CloudNormalMap, material, "CLOUDNORMAL");
                SetShaderKeywordBasedOnBool(_VertexAlphaOn, material, "VERTEXALPHA");
                material.SetInt("_Cull", (int)_Cull);
                material.SetFloat("_ExternalAlpha", _ExternalAlpha);
            }
        }

    }

}