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
    public class HGSnowToppedController : MaterialController
    {

        public Color _Color;
        public Texture _MainTex;
        public Vector2 _MainTexScale;
        public Vector2 _MainTexOffset;

        [Range(0f, 5f)]
        public float _NormalStrength;

        public Texture _NormalTex;
        public Vector2 _NormalTexScale;
        public Vector2 _NormalTexOffset;

        public Texture _SnowTex;
        public Vector2 _SnowTexScale;
        public Vector2 _SnowTexOffset;

        public Texture _SnowNormalTex;
        public Vector2 _SnowNormalTexScale;
        public Vector2 _SnowNormalTexOffset;

        [Range(-1f, 1f)]
        public float _SnowBias;

        [Range(0f, 1f)]
        public float _Depth;

        public bool _IgnoreAlphaWeights;
        public bool _BlendWeightsBinarily;

        public enum _RampInfoEnum
        {
            TwoTone = 0,
            SmoothedTwoTone = 1,
            Unlitish = 3,
            Subsurface = 4,
            Grass = 5
        }
        public _RampInfoEnum _RampChoice;

        public bool _IgnoreDiffuseAlphaForSpeculars;

        [Range(0f, 1f)]
        public float _SpecularStrength;

        [Range(0.1f, 20f)]
        public float _SpecularExponent;

        [Range(0f, 1f)]
        public float _Smoothness;

        [Range(0f, 1f)]
        public float _SnowSpecularStrength;

        [Range(0.1f, 20f)]
        public float _SnowSpecularExponent;

        [Range(0f, 1f)]
        public float _SnowSmoothness;

        public bool _DitherOn;

        public bool _TriplanarOn;

        [Range(0f, 1f)]
        public float _TriplanarTextureFactor;

        public bool _SnowOn;

        public bool _GradientBiasOn;

        public Vector4 _GradientBiasVector;

        public bool __DirtOn;

        public Texture _DirtTex;
        public Vector2 _DirtTexScale;
        public Vector2 _DirtTexOffset;

        public Texture _DirtNormalTex;
        public Vector2 _DirtNormalTexScale;
        public Vector2 _DirtNormalTexOffset;

        [Range(-2f, 2f)]
        public float _DirtBias;

        [Range(0f, 1f)]
        public float _DirtSpecularStrength;

        [Range(0f, 20f)]
        public float _DirtSpecularExponent;

        [Range(0f, 1f)]
        public float _DirtSmoothness;
        public override void GrabMaterialValues()
        {
            if (material)
            {
                _Color = material.GetColor("_Color");
                _MainTex = material.GetTexture("_MainTex");
                _MainTexScale = material.GetTextureScale("_MainTex");
                _MainTexOffset = material.GetTextureOffset("_MainTex");
                _NormalStrength = material.GetFloat("_NormalStrength");
                _NormalTex = material.GetTexture("_NormalTex");
                _NormalTexScale = material.GetTextureScale("_NormalTex");
                _NormalTexOffset = material.GetTextureOffset("_NormalTex");
                _SnowTex = material.GetTexture("_SnowTex");
                _SnowTexScale = material.GetTextureScale("_SnowTex");
                _SnowTexOffset = material.GetTextureOffset("_SnowTex");
                _SnowNormalTex = material.GetTexture("_SnowNormalTex");
                _SnowNormalTexScale = material.GetTextureScale("_SnowNormalTex");
                _SnowNormalTexOffset = material.GetTextureOffset("_SnowNormalTex");
                _SnowBias = material.GetFloat("_SnowBias");
                _Depth = material.GetFloat("_Depth");
                _IgnoreAlphaWeights = material.IsKeywordEnabled("IGNORE_BIAS");
                _BlendWeightsBinarily = material.IsKeywordEnabled("BINARYBLEND");
                _RampChoice = (_RampInfoEnum)(int)material.GetFloat("_RampInfo");
                _IgnoreDiffuseAlphaForSpeculars = material.IsKeywordEnabled("FORCE_SPEC");
                _SpecularStrength = material.GetFloat("_SpecularStrength");
                _SpecularExponent = material.GetFloat("_SpecularExponent");
                _Smoothness = material.GetFloat("_Smoothness");
                _SnowSpecularStrength = material.GetFloat("_SnowSpecularStrength");
                _SnowSpecularExponent = material.GetFloat("_SnowSpecularExponent");
                _SnowSmoothness = material.GetFloat("_SnowSmoothness");
                _DitherOn = material.IsKeywordEnabled("DITHER");
                _TriplanarOn = material.IsKeywordEnabled("TRIPLANAR");
                _TriplanarTextureFactor = material.GetFloat("_TriplanarTextureFactor");
                _SnowOn = material.IsKeywordEnabled("MICROFACET_SNOW");
                _GradientBiasOn = material.IsKeywordEnabled("GRADIENTBIAS");
                _GradientBiasVector = material.GetVector("_GradientBiasVector");
                __DirtOn = material.IsKeywordEnabled("DIRTON");
                _DirtTex = material.GetTexture("_DirtTex");
                _DirtTexScale = material.GetTextureScale("_DirtTex");
                _DirtTexOffset = material.GetTextureOffset("_DirtTex");
                _DirtNormalTex = material.GetTexture("_DirtNormalTex");
                _DirtNormalTexScale = material.GetTextureScale("_DirtNormalTex");
                _DirtNormalTexOffset = material.GetTextureOffset("_DirtNormalTex");
                _DirtBias = material.GetFloat("_DirtBias");
                _DirtSpecularStrength = material.GetFloat("_DirtSpecularStrength");
                _DirtSpecularExponent = material.GetFloat("_DirtSpecularExponent");
                _DirtSmoothness = material.GetFloat("_DirtSmoothness");
            }
        }

        public void Update()
        {
            if (material)
            {

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

                if (_SnowTex)
                {
                    material.SetTexture("_SnowTex", _SnowTex);
                    material.SetTextureScale("_SnowTex", _SnowTexScale);
                    material.SetTextureOffset("_SnowTex", _SnowTexOffset);
                }
                else
                {
                    material.SetTexture("_SnowTex", null);
                }

                if (_SnowNormalTex)
                {
                    material.SetTexture("_SnowNormalTex", _SnowNormalTex);
                    material.SetTextureScale("_SnowNormalTex", _SnowNormalTexScale);
                    material.SetTextureOffset("_SnowNormalTex", _SnowNormalTexOffset);
                }
                else
                {
                    material.SetTexture("_SnowNormalTex", null);
                }

                material.SetFloat("_SnowBias", _SnowBias);
                material.SetFloat("_Depth", _Depth);

                SetShaderKeywordBasedOnBool(_IgnoreAlphaWeights, material, "IGNORE_BIAS");
                SetShaderKeywordBasedOnBool(_BlendWeightsBinarily, material, "BINARYBLEND");

                material.SetFloat("_RampInfo", Convert.ToSingle(_RampChoice));

                SetShaderKeywordBasedOnBool(_IgnoreDiffuseAlphaForSpeculars, material, "FORCE_SPEC");


                material.SetFloat("_SpecularStrength", _SpecularStrength);
                material.SetFloat("_SpecularExponent", _SpecularExponent);
                material.SetFloat("_Smoothness", _Smoothness);

                material.SetFloat("_SnowSpecularStrength", _SnowSpecularStrength);
                material.SetFloat("_SnowSpecularExponent", _SnowSpecularExponent);
                material.SetFloat("_SnowSmoothness", _SnowSmoothness);

                SetShaderKeywordBasedOnBool(_DitherOn, material, "DITHER");

                SetShaderKeywordBasedOnBool(_TriplanarOn, material, "TRIPLANAR");
                material.SetFloat("_TriplanarTextureFactor", _TriplanarTextureFactor);
                SetShaderKeywordBasedOnBool(_SnowOn, material, "MICROFACET_SNOW");

                SetShaderKeywordBasedOnBool(_GradientBiasOn, material, "GRADIENTBIAS");
                material.SetVector("_GradientBiasVector", _GradientBiasVector);
                SetShaderKeywordBasedOnBool(__DirtOn, material, "DIRTON");

                if (_DirtTex)
                {
                    material.SetTexture("_DirtTex", _DirtTex);
                    material.SetTextureScale("_DirtTex", _DirtTexScale);
                    material.SetTextureOffset("_DirtTex", _DirtTexOffset);
                }
                else
                {
                    material.SetTexture("_DirtTex", null);
                }

                if (_DirtNormalTex)
                {
                    material.SetTexture("_DirtNormalTex", _DirtNormalTex);
                    material.SetTextureScale("_DirtNormalTex", _DirtNormalTexScale);
                    material.SetTextureOffset("_DirtNormalTex", _DirtNormalTexOffset);
                }
                else
                {
                    material.SetTexture("_DirtNormalTex", null);
                }

                material.SetFloat("_DirtBias", _DirtBias);
                material.SetFloat("_DirtSpecularStrength", _DirtSpecularStrength);
                material.SetFloat("_DirtSpecularExponent", _DirtSpecularExponent);
                material.SetFloat("_DirtSmoothness", _DirtSmoothness);
            }
        }

    }

}