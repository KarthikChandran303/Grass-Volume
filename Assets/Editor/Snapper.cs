using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Snapper {
    
    const string UNDO_SNAP = "Snap Selected Objects";
    
    [MenuItem("Edit/Snap Selected Objects")]
    public static void SnapSelectedObjects()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, UNDO_SNAP);
            go.transform.position = go.transform.position.Round();
        }
    }

    public static Vector3 Round(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
        return v;
    }
}
