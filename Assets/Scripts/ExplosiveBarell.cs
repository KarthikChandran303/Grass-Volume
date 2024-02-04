using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ExplosiveBarell : MonoBehaviour
{
    public Color color;

    private MaterialPropertyBlock mpb;

    static readonly int shPropColor = Shader.PropertyToID("_BaseColor");
    public MaterialPropertyBlock Mpb
    {
        get
        {
            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }
            return mpb;
        }
    }

    void ApplyColor()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        Mpb.SetColor(shPropColor, color);
        mr.SetPropertyBlock(Mpb);
    }

    private void OnEnable()
    {
        ApplyColor();
        BarellManager.explosiveBarells.Add(this);
    }
    private void OnDisable() => BarellManager.explosiveBarells.Remove(this);

    private void OnDrawGizmos()
    {
    }
}
