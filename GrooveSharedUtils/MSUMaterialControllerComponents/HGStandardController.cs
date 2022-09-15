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
    public class HGStandardController : MaterialController
    {
        public override Shader AssociatedShader => Common.Shaders.standard;

        public bool _EnableCutout;
        public Color _Color;
        public Texture _MainTex;
        public Vector2 _MainTexScale;
        public Vector2 _MainTexOffset;

        [Range(0f, 5f)]
        public float _NormalStrength;

        public Texture _NormalTex;
        public Vector2 _NormalTexScale;
        public Vector2 _NormalTexOffset;
        public Color _EmColor;
        public Texture _EmTex;

        [Range(0f, 10f)]
        public float _EmPower;

        [Range(0f, 1f)]
        public float _Smoothness;

        public bool _IgnoreDiffuseAlphaForSpeculars;

        public enum _RampInfoEnum
        {
            TwoTone = 0,
            SmoothedTwoTone = 1,
            Unlitish = 3,
            Subsurface = 4,
            Grass = 5
        }
        public _RampInfoEnum _RampChoice;

        public enum _DecalLayerEnum
        {
            Default = 0,
            Environment = 1,
            Character = 2,
            Misc = 3
        }
        public _DecalLayerEnum _DecalLayer;

        [Range(0f, 1f)]
        public float _SpecularStrength;

        [Range(0.1f, 20f)]
        public float _SpecularExponent;

        public enum _CullEnum
        {
            Off = 0,
            Front = 1,
            Back = 2
        }
        public _CullEnum _Cull_Mode;

        public bool _EnableDither;

        [Range(0f, 1f)]
        public float _FadeBias;

        public bool _EnableFresnelEmission;

        public Texture _FresnelRamp;

        [Range(0.1f, 20f)]
        public float _FresnelPower;

        public Texture _FresnelMask;

        [Range(0f, 20f)]
        public float _FresnelBoost;

        public bool _EnablePrinting;

        [Range(-25f, 25f)]
        public float _SliceHeight;

        [Range(0f, 10f)]
        public float _PrintBandHeight;

        [Range(0f, 1f)]
        public float _PrintAlphaDepth;

        public Texture _PrintAlphaTexture;
        public Vector2 _PrintAlphaTextureScale;
        public Vector2 _PrintAlphaTextureOffset;

        [Range(0f, 10f)]
        public float _PrintColorBoost;

        [Range(0f, 4f)]
        public float _PrintAlphaBias;

        [Range(0f, 1f)]
        public float _PrintEmissionToAlbedoLerp;

        public enum _PrintDirectionEnum
        {
            BottomUp = 0,
            TopDown = 1,
            BackToFront = 3
        }
        public _PrintDirectionEnum _PrintDirection;

        public Texture _PrintRamp;

        [Range(-10f, 10f)]
        public float _EliteBrightnessMin;

        [Range(-10f, 10f)]
        public float _EliteBrightnessMax;

        public bool _EnableSplatmap;
        public bool _UseVertexColorsInstead;

        [Range(0f, 1f)]
        public float _BlendDepth;

        public Texture _SplatmapTex;
        public Vector2 _SplatmapTexScale;
        public Vector2 _SplatmapTexOffset;

        [Range(0f, 20f)]
        public float _SplatmapTileScale;

        public Texture _GreenChannelTex;
        public Texture _GreenChannelNormalTex;

        [Range(0f, 1f)]
        public float _GreenChannelSmoothness;

        [Range(-2f, 5f)]
        public float _GreenChannelBias;

        public Texture _BlueChannelTex;
        public Texture _BlueChannelNormalTex;

        [Range(0f, 1f)]
        public float _BlueChannelSmoothness;

        [Range(-2f, 5f)]
        public float _BlueChannelBias;

        public bool _EnableFlowmap;
        public Texture _FlowTexture;
        public Texture _FlowHeightmap;
        public Vector2 _FlowHeightmapScale;
        public Vector2 _FlowHeightmapOffset;
        public Texture _FlowHeightRamp;
        public Vector2 _FlowHeightRampScale;
        public Vector2 _FlowHeightRampOffset;

        [Range(-1f, 1f)]
        public float _FlowHeightBias;

        [Range(0.1f, 20f)]
        public float _FlowHeightPower;

        [Range(0.1f, 20f)]
        public float _FlowEmissionStrength;

        [Range(0f, 15f)]
        public float _FlowSpeed;

        [Range(0f, 5f)]
        public float _MaskFlowStrength;

        [Range(0f, 5f)]
        public float _NormalFlowStrength;

        [Range(0f, 10f)]
        public float _FlowTextureScaleFactor;

        public bool _EnableLimbRemoval;
        public override void GrabMaterialValues()
        {
            if (material)
            {
                _EnableCutout = material.IsKeywordEnabled("CUTOUT");
                _Color = material.GetColor("_Color");
                _MainTex = material.GetTexture("_MainTex");
                _MainTexScale = material.GetTextureScale("_MainTex");
                _MainTexOffset = material.GetTextureOffset("_MainTex");
                _NormalStrength = material.GetFloat("_NormalStrength");
                _NormalTex = material.GetTexture("_NormalTex");
                _NormalTexScale = material.GetTextureScale("_NormalTex");
                _NormalTexOffset = material.GetTextureOffset("_NormalTex");
                _EmColor = material.GetColor("_EmColor");
                _EmTex = material.GetTexture("_EmTex");
                _EmPower = material.GetFloat("_EmPower");
                _Smoothness = material.GetFloat("_Smoothness");
                _IgnoreDiffuseAlphaForSpeculars = material.IsKeywordEnabled("FORCE_SPEC");
                _RampChoice = (_RampInfoEnum)(int)material.GetFloat("_RampInfo");
                _DecalLayer = (_DecalLayerEnum)(int)material.GetFloat("_DecalLayer");
                _SpecularStrength = material.GetFloat("_SpecularStrength");
                _SpecularExponent = material.GetFloat("_SpecularExponent");
                _Cull_Mode = (_CullEnum)(int)material.GetFloat("_Cull");
                _EnableDither = material.IsKeywordEnabled("DITHER");
                _FadeBias = material.GetFloat("_FadeBias");
                _EnableFresnelEmission = material.IsKeywordEnabled("FRESNEL_EMISSION");
                _FresnelRamp = material.GetTexture("_FresnelRamp");
                _FresnelPower = material.GetFloat("_FresnelPower");
                _FresnelMask = material.GetTexture("_FresnelMask");
                _FresnelBoost = material.GetFloat("_FresnelBoost");
                _EnablePrinting = material.IsKeywordEnabled("PRINT_CUTOFF");
                _SliceHeight = material.GetFloat("_SliceHeight");
                _PrintBandHeight = material.GetFloat("_SliceBandHeight");
                _PrintAlphaDepth = material.GetFloat("_SliceAlphaDepth");
                _PrintAlphaTexture = material.GetTexture("_SliceAlphaTex");
                _PrintAlphaTextureScale = material.GetTextureScale("_SliceAlphaTex");
                _PrintAlphaTextureOffset = material.GetTextureOffset("_SliceAlphaTex");
                _PrintColorBoost = material.GetFloat("_PrintBoost");
                _PrintAlphaBias = material.GetFloat("_PrintBias");
                _PrintEmissionToAlbedoLerp = material.GetFloat("_PrintEmissionToAlbedoLerp");
                _PrintDirection = (_PrintDirectionEnum)(int)material.GetFloat("_PrintDirection");
                _PrintRamp = material.GetTexture("_PrintRamp");
                _EliteBrightnessMin = material.GetFloat("_EliteBrightnessMin");
                _EliteBrightnessMax = material.GetFloat("_EliteBrightnessMax");
                _EnableSplatmap = material.IsKeywordEnabled("SPLATMAP");
                _UseVertexColorsInstead = material.IsKeywordEnabled("USE_VERTEX_COLORS");
                _BlendDepth = material.GetFloat("_Depth");
                _SplatmapTex = material.GetTexture("_SplatmapTex");
                _SplatmapTexScale = material.GetTextureScale("_SplatmapTex");
                _SplatmapTexOffset = material.GetTextureOffset("_SplatmapTex");
                _SplatmapTileScale = material.GetFloat("_SplatmapTileScale");
                _GreenChannelTex = material.GetTexture("_GreenChannelTex");
                _GreenChannelNormalTex = material.GetTexture("_GreenChannelNormalTex");
                _GreenChannelSmoothness = material.GetFloat("_GreenChannelSmoothness");
                _GreenChannelBias = material.GetFloat("_GreenChannelBias");
                _BlueChannelTex = material.GetTexture("_BlueChannelTex");
                _BlueChannelNormalTex = material.GetTexture("_BlueChannelNormalTex");
                _BlueChannelSmoothness = material.GetFloat("_BlueChannelSmoothness");
                _BlueChannelBias = material.GetFloat("_BlueChannelBias");
                _EnableFlowmap = material.IsKeywordEnabled("FLOWMAP");
                _FlowTexture = material.GetTexture("_FlowTex");
                _FlowHeightmap = material.GetTexture("_FlowHeightmap");
                _FlowHeightmapScale = material.GetTextureScale("_FlowHeightmap");
                _FlowHeightmapOffset = material.GetTextureOffset("_FlowHeightmap");
                _FlowHeightRamp = material.GetTexture("_FlowHeightRamp");
                _FlowHeightRampScale = material.GetTextureScale("_FlowHeightRamp");
                _FlowHeightRampOffset = material.GetTextureOffset("_FlowHeightRamp");
                _FlowHeightBias = material.GetFloat("_FlowHeightBias");
                _FlowHeightPower = material.GetFloat("_FlowHeightPower");
                _FlowEmissionStrength = material.GetFloat("_FlowEmissionStrength");
                _FlowSpeed = material.GetFloat("_FlowSpeed");
                _MaskFlowStrength = material.GetFloat("_FlowMaskStrength");
                _NormalFlowStrength = material.GetFloat("_FlowNormalStrength");
                _FlowTextureScaleFactor = material.GetFloat("_FlowTextureScaleFactor");
                _EnableLimbRemoval = material.IsKeywordEnabled("LIMBREMOVAL");
            }
        }

        public void Update()
        {
            if (material)
            {
                SetShaderKeywordBasedOnBool(_EnableCutout, material, "CUTOUT");

                material.SetColor("_Color", _Color);

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
                    material.SetTextureScale("_NormalTex", _NormalTexScale);
                    material.SetTextureOffset("_NormalTex", _NormalTexOffset);
                }
                else
                {
                    material.SetTexture("_NormalTex", null);
                }

                material.SetColor("_EmColor", _EmColor);

                if (_EmTex)
                {
                    material.SetTexture("_EmTex", _EmTex);
                }
                else
                {
                    material.SetTexture("_EmTex", null);
                }

                material.SetFloat("_EmPower", _EmPower);
                material.SetFloat("_Smoothness", _Smoothness);

                SetShaderKeywordBasedOnBool(_IgnoreDiffuseAlphaForSpeculars, material, "FORCE_SPEC");

                material.SetFloat("_RampInfo", Convert.ToSingle(_RampChoice));
                material.SetFloat("_DecalLayer", Convert.ToSingle(_DecalLayer));
                material.SetFloat("_SpecularStrength", _SpecularStrength);
                material.SetFloat("_SpecularExponent", _SpecularExponent);
                material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));

                SetShaderKeywordBasedOnBool(_EnableDither, material, "DITHER");

                material.SetFloat("_FadeBias", _FadeBias);

                SetShaderKeywordBasedOnBool(_EnableFresnelEmission, material, "FRESNEL_EMISSION");

                if (_FresnelRamp)
                {
                    material.SetTexture("_FresnelRamp", _FresnelRamp);
                }
                else
                {
                    material.SetTexture("_FresnelRamp", null);
                }

                material.SetFloat("_FresnelPower", _FresnelPower);

                if (_FresnelMask)
                {
                    material.SetTexture("_FresnelMask", _FresnelMask);
                }
                else
                {
                    material.SetTexture("_FresnelMask", null);
                }

                material.SetFloat("_FresnelBoost", _FresnelBoost);

                SetShaderKeywordBasedOnBool(_EnablePrinting, material, "PRINT_CUTOFF");

                material.SetFloat("_SliceHeight", _SliceHeight);
                material.SetFloat("_SliceBandHeight", _PrintBandHeight);
                material.SetFloat("_SliceAlphaDepth", _PrintAlphaDepth);

                if (_PrintAlphaTexture)
                {
                    material.SetTexture("_SliceAlphaTex", _PrintAlphaTexture);
                    material.SetTextureScale("_SliceAlphaTex", _PrintAlphaTextureScale);
                    material.SetTextureOffset("_SliceAlphaTex", _PrintAlphaTextureOffset);
                }
                else
                {
                    material.SetTexture("_SliceAlphaTex", null);
                }

                material.SetFloat("_PrintBoost", _PrintColorBoost);
                material.SetFloat("_PrintBias", _PrintAlphaBias);
                material.SetFloat("_PrintEmissionToAlbedoLerp", _PrintEmissionToAlbedoLerp);
                material.SetFloat("_PrintDirection", Convert.ToSingle(_PrintDirection));

                if (_PrintRamp)
                {
                    material.SetTexture("_PrintRamp", _PrintRamp);
                }
                else
                {
                    material.SetTexture("_PrintRamp", null);
                }

                material.SetFloat("_EliteBrightnessMin", _EliteBrightnessMin);
                material.SetFloat("_EliteBrightnessMax", _EliteBrightnessMax);

                SetShaderKeywordBasedOnBool(_EnableSplatmap, material, "SPLATMAP");
                SetShaderKeywordBasedOnBool(_UseVertexColorsInstead, material, "USE_VERTEX_COLORS");

                material.SetFloat("_Depth", _BlendDepth);

                if (_SplatmapTex)
                {
                    material.SetTexture("_SplatmapTex", _SplatmapTex);
                    material.SetTextureScale("_SplatmapTex", _SplatmapTexScale);
                    material.SetTextureOffset("_SplatmapTex", _SplatmapTexOffset);
                }
                else
                {
                    material.SetTexture("_SplatmapTex", null);
                }

                material.SetFloat("_SplatmapTileScale", _SplatmapTileScale);

                if (_GreenChannelTex)
                {
                    material.SetTexture("_GreenChannelTex", _GreenChannelTex);
                }
                else
                {
                    material.SetTexture("_GreenChannelTex", null);
                }

                if (_GreenChannelNormalTex)
                {
                    material.SetTexture("_GreenChannelNormalTex", _GreenChannelNormalTex);
                }
                else
                {
                    material.SetTexture("_GreenChannelNormalTex", null);
                }

                material.SetFloat("_GreenChannelSmoothness", _GreenChannelSmoothness);
                material.SetFloat("_GreenChannelBias", _GreenChannelBias);

                if (_BlueChannelTex)
                {
                    material.SetTexture("_BlueChannelTex", _BlueChannelTex);
                }
                else
                {
                    material.SetTexture("_BlueChannelTex", null);
                }

                if (_BlueChannelNormalTex)
                {
                    material.SetTexture("_BlueChannelNormalTex", _BlueChannelNormalTex);
                }
                else
                {
                    material.SetTexture("_BlueChannelNormalTex", null);
                }

                material.SetFloat("_BlueChannelSmoothness", _BlueChannelSmoothness);
                material.SetFloat("_BlueChannelBias", _BlueChannelBias);

                SetShaderKeywordBasedOnBool(_EnableFlowmap, material, "FLOWMAP");

                if (_FlowTexture)
                {
                    material.SetTexture("_FlowTex", _FlowTexture);
                }
                else
                {
                    material.SetTexture("_FlowTex", null);
                }

                if (_FlowHeightmap)
                {
                    material.SetTexture("_FlowHeightmap", _FlowHeightmap);
                    material.SetTextureScale("_FlowHeightmap", _FlowHeightmapScale);
                    material.SetTextureOffset("_FlowHeightmap", _FlowHeightmapOffset);
                }
                else
                {
                    material.SetTexture("_FlowHeightmap", null);
                }

                if (_FlowHeightRamp)
                {
                    material.SetTexture("_FlowHeightRamp", _FlowHeightRamp);
                    material.SetTextureScale("_FlowHeightRamp", _FlowHeightRampScale);
                    material.SetTextureOffset("_FlowHeightRamp", _FlowHeightRampOffset);
                }
                else
                {
                    material.SetTexture("_FlowHeightRamp", null);
                }

                material.SetFloat("_FlowHeightBias", _FlowHeightBias);
                material.SetFloat("_FlowHeightPower", _FlowHeightPower);
                material.SetFloat("_FlowEmissionStrength", _FlowEmissionStrength);
                material.SetFloat("_FlowSpeed", _FlowSpeed);
                material.SetFloat("_FlowMaskStrength", _MaskFlowStrength);
                material.SetFloat("_FlowNormalStrength", _NormalFlowStrength);
                material.SetFloat("_FlowTextureScaleFactor", _FlowTextureScaleFactor);

                SetShaderKeywordBasedOnBool(_EnableLimbRemoval, material, "LIMBREMOVAL");
            }
        }

    }

}