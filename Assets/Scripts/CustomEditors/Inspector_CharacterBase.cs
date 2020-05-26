using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterBase))]
public class Inspector_CharacterBase : Editor
{
    bool ShowSetup = false;
    bool ShowDamageStuff = false;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginVertical("box");
        ShowSetup = EditorGUILayout.Foldout(ShowSetup, "Setup", true);
        if (ShowSetup)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Renderers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IFramesLayer"));
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("RunSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxHealth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("GroundSnapDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Attacks"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageResistances"));

        EditorGUILayout.BeginVertical("box");
        ShowDamageStuff = EditorGUILayout.Foldout(ShowDamageStuff, "Damage Stuff", true);
        if (ShowDamageStuff)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageFlinchStateName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageSlideStateName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageKnockdownStateName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageFadeTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DeathDamageFadeTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageColor"));
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDeath"));

        EditorGUILayout.EndVertical();
    }
    protected void OnSceneGUI()
    {
        Handles.BeginGUI();
        CharacterBase characterBase = (CharacterBase)target;

        Rect rect = new Rect(20, 20, 100, 30);
        if (GUI.Button(rect, new GUIContent("Show Attacks")))
        {
            foreach (AttackHitbox attackHitbox in characterBase.Attacks)
            {
                attackHitbox.ShowAttack();
            }
        }
        rect = new Rect(20, 65, 100, 30);
        if (GUI.Button(rect, new GUIContent("Hide Attacks")))
        {
            foreach (AttackHitbox attackHitbox in characterBase.Attacks)
            {
                attackHitbox.HideAttack();
            }
        }
        Handles.EndGUI();

    }
}