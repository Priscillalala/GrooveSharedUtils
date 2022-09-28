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
    public class HGWavyClothController : MaterialController
    {
        public override Shader AssociatedShader => Common.Shaders.wavyCloth;

        public Color _Color;
        [Range(0f, 1f)]
        public float _Cutoff;
        public Texture _MainTex;
        public Vector2 _MainTexScale;
        public Vector2 _MainTexOffset;
        public Texture _ScrollingNormalMap;
        public Vector2 _NormalScale;
        public Vector2 _NormalOffset;
        [Range(0f, 5f)]
        public float _NormalStrength;
        public Vector4 _Scroll;
        [Range(0f, 5f)]
        public float _VertexOffsetStrength;
        public Vector4 _WindVector;
        [Range(0f, 1f)]
        public float _Smoothness;
        public enum _RampEnum
        {
            TwoTone = 0,
            SmoothedTwoTone = 1,
            Unlitish = 3,
            Subsurface = 4,
            Grass = 5
        }
        public _RampEnum _RampInfo;
        [Range(0f, 1f)]
        public float _SpecularStrength;
        [Range(0.1f, 20f)]
        public float _SpecularExponent;
        public bool _EnableVertexColorDistortion;

        public override void GrabMaterialValues()
        {
            if (material)
            {
                _Color = material.GetColor("_Color");
                _Cutoff = material.GetFloat("_Cutoff");
                _MainTex = material.GetTexture("_MainTex");
                _MainTexScale = material.GetTextureScale("_MainTex");
                _MainTexOffset = material.GetTextureOffset("_MainTex");
                _ScrollingNormalMap = material.GetTexture("_ScrollingNormalMap");
                _NormalScale = material.GetTextureScale("_ScrollingNormalMap");
                _NormalOffset = material.GetTextureOffset("_ScrollingNormalMap");
                _NormalStrength = material.GetFloat("_NormalStrength");
                _Scroll = material.GetVector("_Scroll");
                _VertexOffsetStrength = material.GetFloat("_VertexOffsetStrength");
                _WindVector = material.GetVector("_WindVector");
                _Smoothness = material.GetFloat("_Smoothness");
                _RampInfo = (_RampEnum)(int)material.GetFloat("_RampInfo");
                _SpecularStrength = material.GetFloat("_SpecularStrength");
                _SpecularExponent = material.GetFloat("_SpecularExponent");
                _EnableVertexColorDistortion = material.IsKeywordEnabled("VERTEX_RED_FOR_DISTORTION");
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

                if (_ScrollingNormalMap)
                {
                    material.SetTexture("_ScrollingNormalMap", _ScrollingNormalMap);
                    material.SetTextureScale("_ScrollingNormalMap", _NormalScale);
                    material.SetTextureOffset("_ScrollingNormalMap", _NormalOffset);
                }
                else
                {
                    material.SetTexture("_ScrollingNormalMap", null);
                }



                material.SetFloat("_Smoothness", _Smoothness);
                material.SetFloat("_Cutoff", _Cutoff);
                material.SetFloat("_SpecularExponent", _SpecularExponent);
                material.SetFloat("_SpecularStrength", _SpecularStrength);
                material.SetFloat("_VertexOffsetStrength", _VertexOffsetStrength);

                material.SetFloat("_RampInfo", Convert.ToSingle(_RampInfo));


                material.SetVector("_Scroll", _Scroll);
                material.SetVector("_WindVector", _WindVector);

                SetShaderKeywordBasedOnBool(_EnableVertexColorDistortion, material, "VERTEX_RED_FOR_DISTORTION");

                material.SetFloat("_NormalStrength", _NormalStrength);
            }
        }

    }

}