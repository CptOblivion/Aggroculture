using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonsterAI))]
public class Inspector_MonsterAI : Inspector_CharacterBase
{
  static bool ShowCharacterBase = true;
  public override void OnInspectorGUI()
  {
    serializedObject.Update();
    EditorGUILayout.BeginVertical("box");
    EditorGUI.indentLevel++;
    ShowCharacterBase = EditorGUILayout.Foldout(ShowCharacterBase, "Character Base Properties", true);
    if (ShowCharacterBase)
    {
      base.OnInspectorGUI();
    }
    EditorGUI.indentLevel--;
    EditorGUILayout.EndVertical();

    EditorGUILayout.PropertyField(serializedObject.FindProperty("TurnSpeed"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("AggroDistance"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreferredCombatDistance"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("CombatDistanceMoveThreshold"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("AttackTriggers"));


    serializedObject.ApplyModifiedProperties();
  }
}