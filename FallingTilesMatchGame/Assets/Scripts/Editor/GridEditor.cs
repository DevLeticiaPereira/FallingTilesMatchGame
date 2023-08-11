using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridSetup))]
public class GridManagerEditor : Editor
{
    SerializedProperty _gridSetupDataProp;

    void OnEnable()
    {
        _gridSetupDataProp = serializedObject.FindProperty("_gridSetupData");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();

        GridSetup gridSetupManager = (GridSetup)target;

        if (GUILayout.Button("Generate Grid"))
        {
            gridSetupManager.GenerateGrid();
        }

        serializedObject.ApplyModifiedProperties();
    }
}