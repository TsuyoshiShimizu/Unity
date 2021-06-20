using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(Snap))]
public class SnapEditor : Editor
{
    private SerializedProperty SnapMoveSpan;
    private SerializedProperty SnapRotSpan;
    private SerializedProperty SnapScaleSpan;
    private GameObject Obj;

    private void OnEnable()
    {
        Snap Object = (Snap)target;
        Obj = Object.transform.gameObject;
        SnapMoveSpan = serializedObject.FindProperty("snapMoveSpan");
        SnapRotSpan = serializedObject.FindProperty("snapRotSpan");
        SnapScaleSpan = serializedObject.FindProperty("snapScaleSpan");
    }

    private void OnSceneGUI()
    {       
        var result = Handles.FreeMoveHandle(Obj.transform.position, Obj.transform.rotation, 1f, Vector3.one, Handles.SphereHandleCap);
        var resultRot = Handles.FreeRotateHandle(Obj.transform.rotation, Obj.transform.position, 0.1f);
        var resultScale = Handles.ScaleHandle(Obj.transform.localScale, Obj.transform.position, Obj.transform.rotation, 1f);
        if (SnapMoveSpan.floatValue > 0)
        {
            result.x = MultipleRound(result.x, SnapMoveSpan.floatValue);
            result.y = MultipleRound(result.y, SnapMoveSpan.floatValue);
            result.z = MultipleRound(result.z, SnapMoveSpan.floatValue);
        }
        Vector3 Rot = resultRot.eulerAngles;
        if (SnapRotSpan.floatValue > 0)
        {
            Rot.x = MultipleRound(Rot.x, SnapRotSpan.floatValue);
            Rot.y = MultipleRound(Rot.y, SnapRotSpan.floatValue);
            Rot.z = MultipleRound(Rot.z, SnapRotSpan.floatValue);
        }
        if(SnapScaleSpan.floatValue > 0)
        {
            resultScale.x = MultipleRound(resultScale.x, SnapScaleSpan.floatValue);
            resultScale.y = MultipleRound(resultScale.y, SnapScaleSpan.floatValue);
            resultScale.z = MultipleRound(resultScale.z, SnapScaleSpan.floatValue);
        }
        Obj.transform.position = result;
        Obj.transform.eulerAngles = Rot;
        Obj.transform.localScale = resultScale;
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(SnapMoveSpan, new GUIContent("スナップ間隔 位置"));
        EditorGUILayout.PropertyField(SnapRotSpan, new GUIContent("スナップ間隔 回転"));
        EditorGUILayout.PropertyField(SnapScaleSpan, new GUIContent("スナップ間隔 サイズ"));
        serializedObject.ApplyModifiedProperties();
    }

    private float MultipleFloor(float value, float multiple)
    {
        return Mathf.Floor(value / multiple) * multiple;
    }

    private float MultipleRound(float value, float multiple)
    {
        return MultipleFloor(value + multiple * 0.5f, multiple);
    }
}
