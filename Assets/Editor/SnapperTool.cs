using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SnapperTool : EditorWindow
{
    private float gridSize = 20;
    private float gridScale = 2.0f;
    
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
        int count = (int)(gridSize/gridScale);
        int bit = (count % 2) + 1;
        float end = gridScale * Mathf.Ceil((count)/2.0f);

        Handles.zTest = CompareFunction.LessEqual;
        
        for (int i = 0; i < count + bit; i++)
        {
            float gap = ((end) * -1) + (i * gridScale);
            Handles.DrawLine(new Vector3(-end,0,gap), new Vector3(end,0,gap));
        }

        for (int i = 0; i < count + bit; i++)
        {
            float gap = ((end) * -1) + (i * gridScale);
            Handles.DrawLine(new Vector3(gap,0,-end), new Vector3(gap,0,end));
        }

        foreach (GameObject go in Selection.gameObjects)
        {
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(go.transform.position.Snap(gridScale), Vector3.up, 0.5f);
        }
    }
    
    private void OnGUI()
    {
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Grid Layout", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                float originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 60.0f;
                gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);
                gridScale = EditorGUILayout.FloatField("Grid Scale", gridScale);
                EditorGUIUtility.labelWidth = originalLabelWidth;
            }
        }
        GUILayout.Space(5.0f);
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
            go.transform.position = go.transform.position.Snap(gridScale);
        }   
    }
    
}
