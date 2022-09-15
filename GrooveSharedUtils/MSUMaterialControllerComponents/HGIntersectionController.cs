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
    public class HGIntersectionController : MaterialController
    {
        public override Shader AssociatedShader => Common.Shaders.intersectionCloudRemap;
        public enum _SrcBlendFloatEnum
        {
            Zero = 0,
            One = 1,
            DstColor = 2,
            SrcColor = 3,
            OneMinusDstColor = 4,
            SrcAlpha = 5,
            OneMinusSrcColor = 6,
            DstAlpha = 7,
            OneMinusDstAlpha = 8,
            SrcAlphaSaturate = 9,
            OneMinusSrcAlpha = 10
        }
        public enum _DstBlendFloatEnum
        {
            Zero = 0,
            One = 1,
            DstColor = 2,
            SrcColor = 3,
            OneMinusDstColor = 4,
            SrcAlpha = 5,
            OneMinusSrcColor = 6,
            DstAlpha = 7,
            OneMinusDstAlpha = 8,
            SrcAlphaSaturate = 9,
            OneMinusSrcAlpha = 10
        }
        public _SrcBlendFloatEnum _Source_Blend_Mode;
        public _DstBlendFloatEnum _Destination_Blend_Mode;

        public Color _Tint;
        public Texture _MainTex;
        public Vector2 _MainTexScale;
        public Vector2 _MainTexOffset;
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
        public float _SoftFactor;

        [Range(0.1f, 20f)]
        public float _SoftPower;

        [Range(0f, 5f)]
        public float _BrightnessBoost;

        [Range(0.1f, 20f)]
        public float _RimPower;

        [Range(0f, 5f)]
        public float _RimStrength;

        [Range(0f, 20f)]
        public float _AlphaBoost;

        [Range(0f, 20f)]
        public float _IntersectionStrength;

        public enum _CullEnum
        {
            Off = 0,
            Front = 1,
            Back = 2
        }
        public _CullEnum _Cull_Mode;

        public bool _FadeFromVertexColorsOn;
        public bool _EnableTriplanarProjectionsForClouds;
        public override void GrabMaterialValues()
        {
            if (material)
            {
                _Source_Blend_Mode = (_SrcBlendFloatEnum)(int)material.GetFloat("_SrcBlendFloat");
                _Destination_Blend_Mode = (_DstBlendFloatEnum)(int)material.GetFloat("_DstBlendFloat");
                _Tint = material.GetColor("_TintColor");
                _MainTex = material.GetTexture("_MainTex");
                _MainTexScale = material.GetTextureScale("_MainTex");
                _MainTexOffset = material.GetTextureOffset("_MainTex");
                _Cloud1Tex = material.GetTexture("_Cloud1Tex");
                _Cloud1TexScale = material.GetTextureScale("_Cloud1Tex");
                _Cloud1TexOffset = material.GetTextureOffset("_Cloud1Tex");
                _Cloud2Tex = material.GetTexture("_Cloud2Tex");
                _Cloud2TexScale = material.GetTextureScale("_Cloud2Tex");
                _Cloud2TexOffset = material.GetTextureOffset("_Cloud2Tex");
                _RemapTex = material.GetTexture("_RemapTex");
                _RemapTexScale = material.GetTextureScale("_RemapTex");
                _RemapTexOffset = material.GetTextureOffset("_RemapTex");
                _CutoffScroll = material.GetVector("_CutoffScroll");
                _SoftFactor = material.GetFloat("_InvFade");
                _SoftPower = material.GetFloat("_SoftPower");
                _BrightnessBoost = material.GetFloat("_Boost");
                _RimPower = material.GetFloat("_RimPower");
                _RimStrength = material.GetFloat("_RimStrength");
                _AlphaBoost = material.GetFloat("_AlphaBoost");
                _IntersectionStrength = material.GetFloat("_IntersectionStrength");
                _Cull_Mode = (_CullEnum)(int)material.GetFloat("_Cull");
                _FadeFromVertexColorsOn = material.IsKeywordEnabled("FADE_FROM_VERTEX_COLORS");
                _EnableTriplanarProjectionsForClouds = material.IsKeywordEnabled("TRIPLANAR");
            }
        }

        public void Update()
        {
            if (material)
            {

                material.SetFloat("_SrcBlendFloat", Convert.ToSingle(_Source_Blend_Mode));
                material.SetFloat("_DstBlendFloat", Convert.ToSingle(_Destination_Blend_Mode));
                material.SetColor("_TintColor", _Tint);
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
                material.SetFloat("_InvFade", _SoftFactor);
                material.SetFloat("_SoftPower", _SoftPower);
                material.SetFloat("_Boost", _BrightnessBoost);
                material.SetFloat("_RimPower", _RimPower);
                material.SetFloat("_RimStrength", _RimStrength);
                material.SetFloat("_AlphaBoost", _AlphaBoost);
                material.SetFloat("_IntersectionStrength", _IntersectionStrength);
                material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));

                SetShaderKeywordBasedOnBool(_FadeFromVertexColorsOn, material, "FADE_FROM_VERTEX_COLORS");
                SetShaderKeywordBasedOnBool(_EnableTriplanarProjectionsForClouds, material, "TRIPLANAR");
            }
        }

    }

}