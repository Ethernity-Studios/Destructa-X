using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Gun)), CanEditMultipleObjects]
public class GunCustomEditor : Editor
{
    Gun gun;

    SerializedProperty primaryFire;
    SerializedProperty secondaryFire;

    SerializedProperty damagesList;
    SerializedProperty stats;
    SerializedProperty scope;
    SerializedProperty gunTransform;
    private void OnEnable()
    {
        gun = (Gun)target;

        primaryFire = serializedObject.FindProperty("PrimaryFire");
        secondaryFire = serializedObject.FindProperty("SecondaryFire");
        damagesList = serializedObject.FindProperty("Damages");

        stats = serializedObject.FindProperty("Stats");
        scope = serializedObject.FindProperty("Scope");
        gunTransform = serializedObject.FindProperty("GunTransform");

    }

    public override void OnInspectorGUI()
    {
        gun.GunID = (int)EditorGUILayout.IntField("GunID", gun.GunID);
        gun.Name = (string)EditorGUILayout.TextField("Name", gun.Name);
        EditorGUILayout.BeginHorizontal();
        gun.GunModel = (GameObject)EditorGUILayout.ObjectField("GunModel", gun.GunModel, typeof(GameObject), true);
        EditorGUILayout.EndHorizontal();
        gun.Price = (int)EditorGUILayout.IntField("Price", gun.Price);
        gun.MagazineAmmo = (int)EditorGUILayout.IntField("MagazineAmmo", gun.MagazineAmmo);
        gun.Ammo = (int)EditorGUILayout.IntField("Ammo", gun.Ammo);
        gun.ReloadTime = (float)EditorGUILayout.FloatField("ReloadTime", gun.ReloadTime);
        gun.EquipTime = (float)EditorGUILayout.FloatField("EqupidTime", gun.EquipTime);
        gun.Type = (GunType)EditorGUILayout.EnumPopup("PlayerType", gun.Type);
        gun.Category = (GunCategory)EditorGUILayout.EnumPopup("GunCategory", gun.Category);
        gun.BulletPenetration = (float)EditorGUILayout.FloatField("BulletPenetration", gun.BulletPenetration);
        EditorGUILayout.PropertyField(primaryFire, new GUIContent("PrimaryFire"));
        gun.HasSecondaryFire = (bool)EditorGUILayout.Toggle("HasSecondaryFire", gun.HasSecondaryFire);
        if(gun.HasSecondaryFire)
        EditorGUILayout.PropertyField(secondaryFire, new GUIContent("SecondaryFire"));
        gun.Bloom = (float)EditorGUILayout.FloatField("Bloom", gun.Bloom);
        gun.Recoil = (float)EditorGUILayout.FloatField("Recoil", gun.Recoil);
        EditorGUILayout.PropertyField(damagesList, new GUIContent("Damages"));
        EditorGUILayout.PropertyField(stats, new GUIContent("Stats"));
        EditorGUILayout.PropertyField(scope, new GUIContent("Scope"));
        EditorGUILayout.PropertyField(gunTransform, new GUIContent("GunTransform"));

        serializedObject.ApplyModifiedProperties();
    }
}



#endif
