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
    public class HGCloudRemapController : MaterialController
    {
        public override Shader AssociatedShader => Common.Shaders.cloudRemap;
        public enum _BlendEnums
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
        public _BlendEnums _SrcBlend;
        public _BlendEnums _DstBlend;

        public Color _Tint;
        public bool _DisableRemapping;
        public Texture _MainTex;
        public Vector2 _MainTexScale;
        public Vector2 _MainTexOffset;
        public Texture _RemapTex;
        public Vector2 _RemapTexScale;
        public Vector2 _RemapTexOffset;

        [Range(0f, 2f)]
        public float _SoftFactor;

        [Range(1f, 20f)]
        public float _BrightnessBoost;

        [Range(0f, 20f)]
        public float _AlphaBoost;

        [Range(0f, 1f)]
        public float _AlphaBias;

        public bool _UseUV1;
        public bool _FadeWhenNearCamera;

        [Range(0f, 1f)]
        public float _FadeCloseDistance;

        public enum _CullEnum
        {
            Off = 0,
            Front = 1,
            Back = 2
        }
        public _CullEnum _Cull_Mode;

        public enum _ZTestEnum
        {
            Disabled = 0,
            Never = 1,
            Less = 2,
            Equal = 3,
            LessEqual = 4,
            Greater = 5,
            NotEqual = 6,
            GreaterEqual = 7,
            Always = 8
        }
        public _ZTestEnum _ZTest_Mode;

        [Range(-10f, 10f)]
        public float _DepthOffset;

        public bool _CloudRemapping;
        public bool _DistortionClouds;

        [Range(-2f, 2f)]
        public float _DistortionStrength;

        public Texture _Cloud1Tex;
        public Vector2 _Cloud1TexScale;
        public Vector2 _Cloud1TexOffset;
        public Texture _Cloud2Tex;
        public Vector2 _Cloud2TexScale;
        public Vector2 _Cloud2TexOffset;
        public Vector4 _CutoffScroll;
        public bool _VertexColors;
        public bool _LuminanceForVertexAlpha;
        public bool _LuminanceForTextureAlpha;
        public bool _VertexOffset;
        public bool _FresnelFade;
        public bool _SkyboxOnly;

        [Range(-20f, 20f)]
        public float _FresnelPower;

        [Range(0f, 3f)]
        public float _VertexOffsetAmount;
        public override void GrabMaterialValues()
        {
            if (material)
            {
                _SrcBlend = (_BlendEnums)(int)material.GetFloat("_SrcBlend");
                _DstBlend = (_BlendEnums)(int)material.GetFloat("_DstBlend");
                _Tint = material.GetColor("_TintColor");
                _DisableRemapping = material.IsKeywordEnabled("DISABLEREMAP");
                _MainTex = material.GetTexture("_MainTex");
                _MainTexScale = material.GetTextureScale("_MainTex");
                _MainTexOffset = material.GetTextureOffset("_MainTex");
                _RemapTex = material.GetTexture("_RemapTex");
                _RemapTexScale = material.GetTextureScale("_RemapTex");
                _RemapTexOffset = material.GetTextureOffset("_RemapTex");
                _SoftFactor = material.GetFloat("_InvFade");
                _BrightnessBoost = material.GetFloat("_Boost");
                _AlphaBoost = material.GetFloat("_AlphaBoost");
                _AlphaBias = material.GetFloat("_AlphaBias");
                _UseUV1 = material.IsKeywordEnabled("USE_UV1");
                _FadeWhenNearCamera = material.IsKeywordEnabled("FADECLOSE");
                _FadeCloseDistance = material.GetFloat("_FadeCloseDistance");
                _Cull_Mode = (_CullEnum)(int)material.GetFloat("_Cull");
                _ZTest_Mode = (_ZTestEnum)(int)material.GetFloat("_ZTest");
                _DepthOffset = material.GetFloat("_DepthOffset");
                _CloudRemapping = material.IsKeywordEnabled("USE_CLOUDS");
                _DistortionClouds = material.IsKeywordEnabled("CLOUDOFFSET");
                _DistortionStrength = material.GetFloat("_DistortionStrength");
                _Cloud1Tex = material.GetTexture("_Cloud1Tex");
                _Cloud1TexScale = material.GetTextureScale("_Cloud1Tex");
                _Cloud1TexOffset = material.GetTextureOffset("_Cloud1Tex");
                _Cloud2Tex = material.GetTexture("_Cloud2Tex");
                _Cloud2TexScale = material.GetTextureScale("_Cloud2Tex");
                _Cloud2TexOffset = material.GetTextureOffset("_Cloud2Tex");
                _CutoffScroll = material.GetVector("_CutoffScroll");
                _VertexColors = material.IsKeywordEnabled("VERTEXCOLOR");
                _LuminanceForVertexAlpha = material.IsKeywordEnabled("VERTEXALPHA");
                _LuminanceForTextureAlpha = material.IsKeywordEnabled("CALCTEXTUREALPHA");
                _VertexOffset = material.IsKeywordEnabled("VERTEXOFFSET");
                _FresnelFade = material.IsKeywordEnabled("FRESNEL");
                _SkyboxOnly = material.IsKeywordEnabled("SKYBOX_ONLY");
                _FresnelPower = material.GetFloat("_FresnelPower");
                _VertexOffsetAmount = material.GetFloat("_OffsetAmount");
            }
        }

        public void Update()
        {
            if (material)
            {

                material.SetFloat("_SrcBlend", Convert.ToSingle(_SrcBlend));
                material.SetFloat("_DstBlend", Convert.ToSingle(_DstBlend));

                material.SetColor("_TintColor", _Tint);

                SetShaderKeywordBasedOnBool(_DisableRemapping, material, "DISABLEREMAP");

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

                material.SetFloat("_InvFade", _SoftFactor);
                material.SetFloat("_Boost", _BrightnessBoost);
                material.SetFloat("_AlphaBoost", _AlphaBoost);
                material.SetFloat("_AlphaBias", _AlphaBias);

                SetShaderKeywordBasedOnBool(_UseUV1, material, "USE_UV1");
                SetShaderKeywordBasedOnBool(_FadeWhenNearCamera, material, "FADECLOSE");

                material.SetFloat("_FadeCloseDistance", _FadeCloseDistance);
                material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));
                material.SetFloat("_ZTest", Convert.ToSingle(_ZTest_Mode));
                material.SetFloat("_DepthOffset", _DepthOffset);

                SetShaderKeywordBasedOnBool(_CloudRemapping, material, "USE_CLOUDS");
                SetShaderKeywordBasedOnBool(_DistortionClouds, material, "CLOUDOFFSET");

                material.SetFloat("_DistortionStrength", _DistortionStrength);

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

                material.SetVector("_CutoffScroll", _CutoffScroll);

                SetShaderKeywordBasedOnBool(_VertexColors, material, "VERTEXCOLOR");
                SetShaderKeywordBasedOnBool(_LuminanceForVertexAlpha, material, "VERTEXALPHA");
                SetShaderKeywordBasedOnBool(_LuminanceForTextureAlpha, material, "CALCTEXTUREALPHA");
                SetShaderKeywordBasedOnBool(_VertexOffset, material, "VERTEXOFFSET");
                SetShaderKeywordBasedOnBool(_FresnelFade, material, "FRESNEL");
                SetShaderKeywordBasedOnBool(_SkyboxOnly, material, "SKYBOX_ONLY");

                material.SetFloat("_FresnelPower", _FresnelPower);
                material.SetFloat("_OffsetAmount", _VertexOffsetAmount);
            }
        }

    }

}