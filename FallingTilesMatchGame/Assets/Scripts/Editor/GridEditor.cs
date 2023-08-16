using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridSetup))]
public class GridManagerEditor : Editor
{
    private SerializedProperty _gridSetupDataProp;

    private void OnEnable()
    {
        _gridSetupDataProp = serializedObject.FindProperty("_gridSetupData");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();

        var gridSetupManager = (GridSetup)target;

        if (GUILayout.Button("Generate Grid")) gridSetupManager.GenerateGrid();

        serializedObject.ApplyModifiedProperties();
    }
}