using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

[ExecuteAlways]
public class ExplosiveBarell : MonoBehaviour
{
    public BarrelType type;

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

    public void TryApplyColor()
    {
        if(type == null)
        {
            return;
        }
        MeshRenderer mr = GetComponent<MeshRenderer>();
        Mpb.SetColor(shPropColor, type.color);
        mr.SetPropertyBlock(Mpb);
    }

    private void OnEnable()
    {
        TryApplyColor();
        BarellManager.explosiveBarells.Add(this);
    }
    private void OnDisable() => BarellManager.explosiveBarells.Remove(this);
    private void OnValidate() => TryApplyColor();

    void OnDrawGizmosSelected()
    {
        if(type == null)
        {
            return;
        }
        
    }

    private void OnDrawGizmos()
    {
        if(type == null)
        {
            return;
        }
        Handles.color = type.color;
        Handles.DrawWireDisc(transform.position, transform.up, type.radius);
    }
}
