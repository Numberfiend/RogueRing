using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyCombat), true)]
public class EnemyCombatEditor : Editor
{
    SerializedProperty enemyData;
    SerializedProperty activeWeaponIndex;

    private void OnEnable()
    {
        enemyData =
            serializedObject.FindProperty("enemyData");

        activeWeaponIndex =
            serializedObject.FindProperty("activeWeaponIndex");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Enemy Data
        EditorGUILayout.PropertyField(enemyData);

        EnemyData data =
            enemyData.objectReferenceValue as EnemyData;

        if (data != null &&
            data.availibleWeapons != null &&
            data.availibleWeapons.Length > 0)
        {
            string[] weaponNames =
                new string[data.availibleWeapons.Length];

            for (int i = 0;
                 i < data.availibleWeapons.Length;
                 i++)
            {
                if (data.availibleWeapons[i] != null)
                {
                    weaponNames[i] =
                        data.availibleWeapons[i].weaponName;
                }
                else
                {
                    weaponNames[i] = "Empty";
                }
            }

            activeWeaponIndex.intValue =
                EditorGUILayout.Popup(
                    "Active Weapon",
                    activeWeaponIndex.intValue,
                    weaponNames
                );

            activeWeaponIndex.intValue =
                Mathf.Clamp(
                    activeWeaponIndex.intValue,
                    0,
                    data.availibleWeapons.Length - 1
                );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Assign an EnemyData with available weapons.",
                MessageType.Warning
            );
        }
        EditorGUILayout.Space();

        // Draw everything else from GruntCombat / EliteCombat
        DrawPropertiesExcluding(
            serializedObject,
            "enemyData",
            "activeWeaponIndex",
            "m_Script"
        );

        serializedObject.ApplyModifiedProperties();
    }
}