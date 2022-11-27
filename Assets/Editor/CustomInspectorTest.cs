using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(InspTest)), CanEditMultipleObjects]
public class CustomInspectorTest : Editor
{
    InspTest Insp;

    SerializedProperty Insane;
    private void OnEnable()
    {
        Insp = (InspTest)target;

        Insane = serializedObject.FindProperty("Gej");
    }

    public override void OnInspectorGUI()
    {
        Insp.Name = (string)EditorGUILayout.TextField("Name", Insp.Name);
        EditorGUILayout.PropertyField(Insane, new GUIContent("Insane"));


        serializedObject.ApplyModifiedProperties();
    }
}

#endif