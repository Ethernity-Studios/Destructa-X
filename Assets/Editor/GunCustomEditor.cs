using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Gun)), CanEditMultipleObjects]
public class GunCustomEditor : Editor
{
    Gun gun;

    SerializedProperty primaryFire;
    private void OnEnable()
    {
        gun = (Gun)target;

        primaryFire = serializedObject.FindProperty("PrimaryFire");
    }

    public override void OnInspectorGUI()
    {
        gun.GunID = (int)EditorGUILayout.IntField("GunID", gun.GunID);
        gun.Name = (string)EditorGUILayout.TextField("Name", gun.Name);
        gun.GunModel = (GameObject)EditorGUILayout.ObjectField("GunModel", gun.GunModel, typeof(GameObject), true);
        EditorGUILayout.PropertyField(primaryFire, new GUIContent("PrimaryFire"));
        gun.Type = (GunType)EditorGUILayout.EnumPopup("PlayerType", gun.Type);


        serializedObject.ApplyModifiedProperties();
    }
}

#endif
