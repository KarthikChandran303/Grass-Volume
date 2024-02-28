using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class SnapperTool : EditorWindow
{
    [MenuItem("Tools/Snapper")]
    public static void OpenTheThing() => GetWindow<SnapperTool>();

    private void OnEnable()
    {
        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        int gridSize = 10;
        float gridScale = 0.65f;
        int count = (int)(gridSize/gridScale);
        float end = ((gridScale * (int)(gridSize/gridScale))/2.0f);
        
        for (int i = 1; i < count; i++)
        {
            float gap = ((end) * -1) + (i * gridScale);
            Handles.DrawLine(new Vector3(-end,0,gap), new Vector3(end,0,gap));
        }

        for (int i = 1; i < count; i++)
        {
            float gap = ((end) * -1) + (i * gridScale);
            Handles.DrawLine(new Vector3(gap,0,-end), new Vector3(gap,0,end));
        }
    }
    
    private void OnGUI()
    {
        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Snap Selected Objects"))
            {
                SnapSelection();
            }
        }
    }

    private void SnapSelection()
    {
        foreach(GameObject go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Snap Selected Objects");
            go.transform.position = go.transform.position.Round();
        }   
    }
}
