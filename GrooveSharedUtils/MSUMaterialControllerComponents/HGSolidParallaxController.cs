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
    public class HGSolidParallaxController : MaterialController
    {
        public override Shader AssociatedShader => Common.Shaders.solidParallax;

        public Vector4 _Color;

        public Texture _MainTex;
        public Vector2 _MainTexScale;
        public Vector2 _MainTexOffset;

        public Texture _EmissionTex;
        public Vector2 _EmissionTexScale;
        public Vector2 _EmissionTexOffset;
        [Range(0.1f, 20f)]
        public float _EmissionPower;

        public Texture _Normal;
        public Vector2 _NormalScale;
        public Vector2 _NormalOffset;

        [Range(0f, 1f)]
        public float _SpecularStrength;
        [Range(0.1f, 20f)]
        public float _SpecularExponent;

        [Range(0f, 1f)]
        public float _Smoothness;

        public Texture _Height1;
        public Vector2 _Height1Scale;
        public Vector2 _Height1Offset;
        public Texture _Height2;
        public Vector2 _Height2Scale;
        public Vector2 _Height2Offset;

        [Range(0, 20f)]
        public float _HeightStrength;
        [Range(0f, 1f)]
        public float _HeightBias;

        public Vector4 _ScrollSpeed;

        public float _Parallax;
        public enum _RampEnum
        {
            TwoTone = 0,
            SmoothedTwoTone = 1,
            Unlitish = 3,
            Subsurface = 4,
            Grass = 5
        }
        public _RampEnum _RampInfo;
        public enum _CullEnum
        {
            Off = 0,
            Front = 1,
            Back = 2
        }
        public _CullEnum _Cull_Mode;

        public bool _AlphaClip;

        public override void GrabMaterialValues()
        {
            if (material)
            {
                _Color = material.GetColor("_Color");
                _MainTex = material.GetTexture("_MainTex");
                _MainTexScale = material.GetTextureScale("_MainTex");
                _MainTexOffset = material.GetTextureOffset("_MainTex");
                _EmissionTex = material.GetTexture("_EmissionTex");
                _EmissionTexScale = material.GetTextureScale("_EmissionTex");
                _EmissionTexOffset = material.GetTextureOffset("_EmissionTex");
                _EmissionPower = material.GetFloat("_EmissionPower");
                _Normal = material.GetTexture("_Normal");
                _NormalScale = material.GetTextureScale("_Normal");
                _NormalOffset = material.GetTextureOffset("_Normal");
                _Smoothness = material.GetFloat("_Smoothness");
                _SpecularStrength = material.GetFloat("_SpecularStrength");
                _SpecularExponent = material.GetFloat("_SpecularExponent");
                _Cull_Mode = (_CullEnum)(int)material.GetFloat("_Cull");
                _RampInfo = (_RampEnum)(int)material.GetFloat("_RampInfo");
                _Height1 = material.GetTexture("_Height1");
                _Height1Scale = material.GetTextureScale("_Height1");
                _Height1Offset = material.GetTextureOffset("_Height1");
                _Height2 = material.GetTexture("_Height2");
                _Height2Scale = material.GetTextureScale("_Height2");
                _Height2Offset = material.GetTextureOffset("_Height2");
                _HeightStrength = material.GetFloat("_HeightStrength");
                _HeightBias = material.GetFloat("_HeightBias");
                _Parallax = material.GetFloat("_Parallax");
                _ScrollSpeed = material.GetVector("_ScrollSpeed");
                _AlphaClip = material.IsKeywordEnabled("ALPHACLIP");
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

                if (_EmissionTex)
                {
                    material.SetTexture("_EmissionTex", _EmissionTex);
                    material.SetTextureScale("_EmissionTex", _EmissionTexScale);
                    material.SetTextureOffset("_EmissionTex", _EmissionTexOffset);
                }
                else
                {
                    material.SetTexture("_EmissionTex", null);
                }

                material.SetFloat("_Smoothness", _Smoothness);
                material.SetFloat("_EmissionPower", _EmissionPower);
                material.SetFloat("_SpecularExponent", _SpecularExponent);
                material.SetFloat("_SpecularStrength", _SpecularStrength);

                material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));
                material.SetFloat("_RampInfo", Convert.ToSingle(_RampInfo));

                material.SetFloat("_HeightBias", _HeightBias);

                if (_Height1)
                {
                    material.SetTexture("_Height1", _Height1);
                    material.SetTextureScale("_Height1", _Height1Scale);
                    material.SetTextureOffset("_Height1", _Height1Offset);
                }
                else
                {
                    material.SetTexture("_Height1", null);
                }

                if (_Height2)
                {
                    material.SetTexture("_Height2", _Height2);
                    material.SetTextureScale("_Height2", _Height2Scale);
                    material.SetTextureOffset("_Height2", _Height2Offset);
                }
                else
                {
                    material.SetTexture("_Height2", null);
                }

                material.SetVector("_ScrollSpeed", _ScrollSpeed);

                SetShaderKeywordBasedOnBool(_AlphaClip, material, "ALPHACLIP");

                material.SetFloat("_HeightStrength", _HeightStrength);
                material.SetFloat("_Parallax", _Parallax);
            }
        }

    }

}