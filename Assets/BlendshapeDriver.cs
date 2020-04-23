using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SkinnedMeshRenderer))]
[ExecuteInEditMode]
public class BlendshapeDriver : MonoBehaviour
{
    public enum DriverSources { PosX,PosY,PosZ,RotX,RotY,RotZ,ScaleX,ScaleY,ScaleZ};
    public Transform driverObject;
    public DriverSources DriverSource;
    public bool Local = true;
    public float Offset = 0;
    public float PropertyScale = 1;

    public int BlendShapeIndex;

    SkinnedMeshRenderer mesh;

    private void OnEnable()
    {
        mesh = GetComponent<SkinnedMeshRenderer>();
    }

    void Update()
    {
        if (driverObject)
        {
            switch (DriverSource)
            {
                case DriverSources.PosX:
                    if (Local) mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.localPosition.x));
                    else mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.position.x));
                    break;
                case DriverSources.PosY:
                    if (Local) mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.localPosition.y));
                    else mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.position.y));
                    break;
                case DriverSources.PosZ:
                    if (Local) mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.localPosition.z));
                    else mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.position.z));
                    break;
                case DriverSources.RotX:
                    if (Local) mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.localRotation.x));
                    else mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.rotation.x));
                    break;
                case DriverSources.RotY:
                    if (Local) mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.localRotation.y));
                    else mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.rotation.y));
                    break;
                case DriverSources.RotZ:
                    if (Local) mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.localRotation.z));
                    else mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.rotation.z));
                    break;
                case DriverSources.ScaleX:
                    if (Local) mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.localScale.x));
                    else mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.lossyScale.x));
                    break;
                case DriverSources.ScaleY:
                    if (Local) mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.localScale.y));
                    else mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.lossyScale.y));
                    break;
                case DriverSources.ScaleZ:
                    if (Local) mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.localScale.z));
                    else mesh.SetBlendShapeWeight(BlendShapeIndex, CalculateWeight(driverObject.lossyScale.z));
                    break;
            }
        }
    }

    float CalculateWeight(float input)
    {
        return ((input - Offset) * PropertyScale);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BlendshapeDriver))]
public class BlendShapeDriverEditor : Editor
{
    SerializedProperty m_driverObject;
    SerializedProperty m_DriverSource;
    SerializedProperty m_Local;
    SerializedProperty m_Offset;

    private void OnEnable()
    {
        m_driverObject = serializedObject.FindProperty("driverObject");
        m_DriverSource = serializedObject.FindProperty("DriverSource");
        m_Local = serializedObject.FindProperty("Local");
        m_Offset = serializedObject.FindProperty("Offset");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        if (m_driverObject.objectReferenceValue as Transform != null)
        {
            if (GUILayout.Button("Set Offset"))
            {
                Transform driverObject = m_driverObject.objectReferenceValue as Transform;
                bool Local = m_Local.boolValue;
                switch (m_DriverSource.enumValueIndex)
                {
                    case 0: //PosX
                        if (Local) m_Offset.floatValue = driverObject.localPosition.x;
                        else m_Offset.floatValue = driverObject.position.x;
                        break;
                    case 1: //PosY
                        if (Local) m_Offset.floatValue = driverObject.localPosition.y;
                        else m_Offset.floatValue = driverObject.position.y;
                        break;
                    case 2: //PosZ
                        if (Local) m_Offset.floatValue = driverObject.localPosition.z;
                        else m_Offset.floatValue = driverObject.position.z;
                        break;
                    case 3: //RotX
                        if (Local) m_Offset.floatValue = driverObject.localRotation.x;
                        else m_Offset.floatValue = driverObject.rotation.x;
                        break;
                    case 4: //RotY
                        if (Local) m_Offset.floatValue = driverObject.localRotation.y;
                        else m_Offset.floatValue = driverObject.rotation.y;
                        break;
                    case 5: //RotZ
                        if (Local) m_Offset.floatValue = driverObject.localRotation.z;
                        else m_Offset.floatValue = driverObject.rotation.z;
                        break;
                    case 6: //ScaleX
                        if (Local) m_Offset.floatValue = driverObject.localScale.x;
                        else m_Offset.floatValue = driverObject.lossyScale.x;
                        break;
                    case 7: //ScaleY
                        if (Local) m_Offset.floatValue = driverObject.localScale.y;
                        else m_Offset.floatValue = driverObject.lossyScale.y;
                        break;
                    case 8: //ScaleZ
                        if (Local) m_Offset.floatValue = driverObject.localScale.z;
                        else m_Offset.floatValue = driverObject.lossyScale.z;
                        break;
                }

            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
