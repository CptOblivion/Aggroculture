using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AttackContainer))]
public class PropertyDrawer_AttackContainer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        EditorGUI.BeginProperty(position, label, property);

        float OldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.LabelField("Attack Details", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Knockback"));
            SerializedProperty currentProp = property.FindPropertyRelative("Damage");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Damage");
                    if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                    {
                        currentProp.InsertArrayElementAtIndex(currentProp.arraySize);
                    }
                    if (currentProp.arraySize > 0)
                    {
                        if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                        {
                            currentProp.DeleteArrayElementAtIndex(currentProp.arraySize - 1);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 100;
                foreach (SerializedProperty p in currentProp)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PropertyField(p.FindPropertyRelative("Type"),GUIContent.none);
                        EditorGUILayout.PropertyField(p.FindPropertyRelative("Amount"), GUIContent.none);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        EditorGUIUtility.labelWidth = OldLabelWidth;
        EditorGUI.EndProperty();
    }
}

[CustomEditor(typeof(AttackHitbox))]
public class Inspector_AttackHitbox : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
    private void OnSceneGUI()
    {
        Handles.BeginGUI();
        Rect rect = new Rect(20, 20, 100, 30);
        AttackHitbox attackhitbox = (AttackHitbox)target;
        if (attackhitbox.AttackEffect && attackhitbox.AttackEffect.activeInHierarchy)
        {
            if (GUI.Button(rect, new GUIContent("Hide Attack")) && !attackhitbox.EffectIsPrefab && !EditorApplication.isPlaying)
            {
                attackhitbox.HideAttack();
            }

        }
        else
        {
            if (GUI.Button(rect, new GUIContent("Show Attack")) && !attackhitbox.EffectIsPrefab && !EditorApplication.isPlaying)
            {
                attackhitbox.ShowAttack();
            }
        }
        Handles.EndGUI();
    }
}
