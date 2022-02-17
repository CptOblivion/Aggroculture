using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AttackHitEffect))]
public class PropertyDrawer_AttackHitEffect : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginProperty(position, label, property);

    //TODO: get rid of the weird extra line before propertyfields start
    float oldWidth = EditorGUIUtility.labelWidth;
    EditorGUIUtility.labelWidth = 100;

    EditorGUILayout.PropertyField(property.FindPropertyRelative("Material"));

    SerializedProperty currentProp = property.FindPropertyRelative("Effect");
    EditorGUILayout.PropertyField(currentProp);
    EditorGUI.BeginDisabledGroup(currentProp.objectReferenceValue == null);
    {
      EditorGUILayout.PropertyField(property.FindPropertyRelative("PointAtSource"));
      EditorGUILayout.PropertyField(property.FindPropertyRelative("StayUpright"));
    }
    EditorGUI.EndDisabledGroup();

    EditorGUIUtility.labelWidth = oldWidth;

    EditorGUI.EndProperty();
  }
}

[CustomEditor(typeof(AttackHitEffectsSet))]
public class Inspector_AttackHitEffect : Editor
{
  SerializedProperty currentProperty;
  int SelectedProperty = 0;
  GUIStyle buttonStyle;
  readonly GUILayoutOption[] DontExpand = new GUILayoutOption[] { GUILayout.ExpandWidth(false) };
  readonly GUILayoutOption[] TabbedSidebarFrame = new GUILayoutOption[] { GUILayout.MinHeight(84), GUILayout.MaxWidth(100) };

  public override void OnInspectorGUI()
  {
    currentProperty = serializedObject.FindProperty("HitEffects");
    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
    {
      if (currentProperty.arraySize > 0)
      {
        EditorGUILayout.BeginHorizontal();
        {
          GUI.SetNextControlName("this button"); //see hacky hack in a few lines
          if (GUILayout.Button(" + ", DontExpand))
          {
            currentProperty.InsertArrayElementAtIndex(SelectedProperty + 1);
            SelectedProperty++;
            //hacky hack to deselect the text field if it's currently active
            GUI.FocusControl("this button");
          }
          if (GUILayout.Button(" - ", DontExpand))
          {
            currentProperty.DeleteArrayElementAtIndex(SelectedProperty);
            if (SelectedProperty >= currentProperty.arraySize)
              SelectedProperty--;
          }
        }
        EditorGUILayout.EndHorizontal();

      }
      else
      {
        GUILayout.BeginHorizontal();
        {
          if (GUILayout.Button(" + ", GUILayout.ExpandWidth(false)))
          {
            currentProperty.InsertArrayElementAtIndex(0);
            SelectedProperty = 0;
          }
          EditorGUI.BeginDisabledGroup(true);
          GUILayout.Button(" - ", GUILayout.ExpandWidth(false));
          EditorGUI.EndDisabledGroup();
        }
        GUILayout.EndHorizontal();
      }
      EditorGUILayout.BeginHorizontal();
      {
        EditorGUILayout.BeginVertical();
        {
          if (currentProperty.arraySize > 0)
          {
            EditorGUILayout.BeginVertical("box", TabbedSidebarFrame);
            {
              EditorGUI.indentLevel++;
              for (int i = 0; i < currentProperty.arraySize; i++)
              {
                if (SelectedProperty == i)
                {
                  buttonStyle = new GUIStyle(EditorStyles.miniButton) { fontStyle = FontStyle.Bold };
                  GUILayout.Label(currentProperty.GetArrayElementAtIndex(i).displayName, buttonStyle, DontExpand);
                }
                else
                {
                  buttonStyle = EditorStyles.miniButtonRight;
                  if (GUILayout.Button(currentProperty.GetArrayElementAtIndex(i).displayName, buttonStyle, DontExpand))
                  {
                    SelectedProperty = i;
                  }
                }
              }
              EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
          }
          else
          {
            EditorGUILayout.BeginVertical("box", TabbedSidebarFrame);
            {
              EditorGUILayout.LabelField("Press + to add an entry");
            }
            EditorGUILayout.EndVertical();
          }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        {
          EditorGUILayout.BeginVertical("box");
          {
            if (currentProperty.arraySize > 0)
            {
              EditorGUILayout.PropertyField(currentProperty.GetArrayElementAtIndex(SelectedProperty));
            }
            else
            {
              EditorGUILayout.Space(20);
              EditorGUILayout.LabelField("--");
              EditorGUILayout.LabelField("~~~");
              EditorGUILayout.LabelField("--");
              EditorGUILayout.Space(20);
            }
          }
          EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
      }
      EditorGUILayout.EndHorizontal();
    }
    EditorGUILayout.EndVertical();

    serializedObject.ApplyModifiedProperties();
  }
}
