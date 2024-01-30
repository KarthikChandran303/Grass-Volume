using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class BarellManager : MonoBehaviour
{
    public static List<ExplosiveBarell> explosiveBarells = new List<ExplosiveBarell>();
    
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        foreach( ExplosiveBarell barell in BarellManager.explosiveBarells )
        {
            float midHeight = (transform.position.y - barell.transform.position.y) * 0.5f;
            Vector3 offset = Vector3.up * midHeight;
            Handles.DrawBezier(transform.position, barell.transform.position, transform.position - offset, barell.transform.position + offset, Color.cyan, EditorGUIUtility.whiteTexture, 1f);
        }
    }
    #endif
}


