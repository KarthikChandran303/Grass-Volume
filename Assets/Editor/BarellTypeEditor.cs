using System.Collections;
using System.Collections.Generic;
using Codice.CM.SEIDInfo;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BarrelType))]
public class BarellTypeEditor : Editor
{
   SerializedProperty radius;
   SerializedProperty damage;
   SerializedProperty color;
   private SerializedObject so;
   
   private void OnEnable()
   {
      so = serializedObject;
      radius = so.FindProperty("radius");
      damage = so.FindProperty("damage");
      color = so.FindProperty("color");
   }
   public override void OnInspectorGUI()
   {
       so.Update();
       EditorGUILayout.PropertyField(radius);
       EditorGUILayout.PropertyField(damage);
       EditorGUILayout.PropertyField(color);
       if (so.ApplyModifiedProperties())
       {
           BarellManager.UpdateAllBarrelColors();
       }
   }
}
