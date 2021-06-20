using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(GridController))]
public class GridEditor : Editor
{
    private AnimBool GridBool;
    private bool GridFlag = true;
    private float GridSpe = 0.5f;

    private GameObject Obj;

    private void OnEnable()
    {
        GridController Object = (GridController)target;
        Obj = Object.transform.gameObject;

        GridBool = new AnimBool(GridFlag);
        GridBool.valueChanged.AddListener(Repaint);
    }

    private void OnSceneGUI()
    {
        var result = Handles.FreeMoveHandle(Obj.transform.position, Obj.transform.rotation, 1f, Vector3.one, Handles.SphereHandleCap);
        if (GridFlag)
        {
            result.x = MultipleRound(result.x, GridSpe);
            result.y = MultipleRound(result.y, GridSpe);
            result.z = MultipleRound(result.z, GridSpe);
        }
        Obj.transform.position = result;
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GridBool.target = EditorGUILayout.ToggleLeft("グリッド", GridBool.target);
        GridFlag = GridBool.target;
        if (EditorGUILayout.BeginFadeGroup(GridBool.faded))
        {
            GridSpe = EditorGUILayout.FloatField("グリッド間隔", GridSpe);
        }
        EditorGUILayout.EndFadeGroup();

      //  EditorGUILayout.PropertyField(ObjectPos, new GUIContent("オブジェクトの位置", "オブジェクトの位置座標の変更、表示を行う"));
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
