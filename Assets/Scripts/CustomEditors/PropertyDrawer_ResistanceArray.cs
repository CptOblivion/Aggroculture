using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ResistanceArray))]
public class PropertyDrawer_ResistanceArray : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginProperty(position, label, property);

    EditorGUILayout.BeginVertical("box");
    {
      EditorGUI.indentLevel--;
      EditorGUILayout.LabelField("Resistances", EditorStyles.boldLabel);
      EditorGUI.indentLevel++;
      float OldLabelWidth = EditorGUIUtility.labelWidth;

      EditorGUIUtility.labelWidth = 100;
      EditorGUILayout.PropertyField(property.FindPropertyRelative("Knockback"));
      EditorGUIUtility.labelWidth = 80;
      GUILayout.BeginHorizontal();
      {
        EditorGUILayout.PropertyField(property.FindPropertyRelative("Cutting"), GUILayout.MinWidth(20));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("Crushing"), GUILayout.MinWidth(20));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("Skewering"), GUILayout.MinWidth(20));
      }
      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal();
      {
        EditorGUILayout.PropertyField(property.FindPropertyRelative("Spicy"), GUILayout.MinWidth(20));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("Sour"), GUILayout.MinWidth(20));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("Salty"), GUILayout.MinWidth(20));
      }
      GUILayout.EndHorizontal();
      EditorGUIUtility.labelWidth = OldLabelWidth;
    }
    EditorGUILayout.EndVertical();

    EditorGUI.EndProperty();

  }
}
