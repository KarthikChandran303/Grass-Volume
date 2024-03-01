using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class VectorExtension
{
    public static Vector3 Round(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
        return v;
    }

    public static Vector3 Snap(this Vector3 v, float snapValue)
    {
        v.x = Mathf.Floor((v.x / snapValue) + 0.5f) * snapValue;
        v.y = 0.0f;
        v.z = Mathf.Floor((v.z / snapValue) + 0.5f) * snapValue;
        return v;
    }
}