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
    public abstract class MaterialController : MonoBehaviour
    {
        public Material material;
        public static void SetShaderKeywordBasedOnBool(bool enabled, Material material, string keyword)
        {
            if (!material)
            {
                return;
            }

            if (enabled)
            {
                if (!material.IsKeywordEnabled(keyword))
                {
                    material.EnableKeyword(keyword);
                }
            }
            else
            {
                if (material.IsKeywordEnabled(keyword))
                {
                    material.DisableKeyword(keyword);
                }
            }
        }
        public virtual void Start()
        {
            if (!material) 
            {
                Renderer renderer = base.GetComponent<Renderer>() ?? base.GetComponentInChildren<Renderer>();
                material = renderer ? renderer.material : null;
            }
            GrabMaterialValues();
        }
        public abstract void GrabMaterialValues();
    }
}