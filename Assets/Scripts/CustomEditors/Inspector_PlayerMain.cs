using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerMain))]
public class PlayerMainInspector : Inspector_CharacterBase
{
  static bool ShowCharacterBase = true;
  static bool ShowSetupPlayer = false;

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
    EditorGUILayout.BeginVertical("box");
    EditorGUI.indentLevel++;
    ShowSetupPlayer = EditorGUILayout.Foldout(ShowSetupPlayer, "Player Setup", true);
    if (ShowSetupPlayer)
    {
      EditorGUILayout.BeginVertical(EditorStyles.helpBox);

      EditorGUILayout.PropertyField(serializedObject.FindProperty("LeftHand"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("RightHand"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("SpinBone"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("FacingObject"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("Hat"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("Tools_Empty"));

      EditorGUILayout.EndVertical();
    }
    EditorGUI.indentLevel--;
    EditorGUILayout.EndVertical();


    EditorGUILayout.PropertyField(serializedObject.FindProperty("NoInteractionIfTooFar"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("InteractFromTileCenter"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("StartingInventory"));
    /*
    EditorGUILayout.BeginVertical("box");
    {
        EditorGUILayout.LabelField("Hotbar");
        currentProperty = serializedObject.FindProperty("Hotbar");

        EditorGUILayout.PropertyField(currentProperty.GetArrayElementAtIndex(0), new GUIContent("Empty Hand"));
        EditorGUILayout.Space();
        for (int i = 1; i < currentProperty.arraySize; i++)
        {
            EditorGUILayout.PropertyField(currentProperty.GetArrayElementAtIndex(i), new GUIContent($"Slot {i}"));
        }
    }
    EditorGUILayout.EndVertical();
    */

    EditorGUILayout.PropertyField(serializedObject.FindProperty("ToolDistanceFar"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("GrabDistanceFar"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("InteractDistance"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("ExitCombatCooldown"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("DodgeRollCooldown"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxTurnSpeed"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxTurnSpeedAttack"));

    serializedObject.ApplyModifiedProperties();
  }
}
