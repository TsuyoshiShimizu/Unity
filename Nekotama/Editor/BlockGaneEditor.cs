using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(BlockGenerator))]
public class BlockGaneEditor : Editor
{
    private AnimBool GridBool;

    private SerializedProperty XN;
    private SerializedProperty YN;
    private SerializedProperty ZN;
    private SerializedProperty Element;
    private SerializedProperty Type;
    private SerializedProperty DType;
    private SerializedProperty VType;
    private SerializedProperty DamageValue;
    private SerializedProperty Alpha;
    private SerializedProperty PMat;
    private SerializedProperty ViewBlock;
    private GameObject Block;
    private BlockGenerator BG;

    private bool GridFlag = true;
    private float GridSpe = 0.5f;

    private void OnEnable()
    {
        XN = serializedObject.FindProperty("XN");
        YN = serializedObject.FindProperty("YN");
        ZN = serializedObject.FindProperty("ZN");
        Element = serializedObject.FindProperty("Element");
        Type = serializedObject.FindProperty("Type");
        DType = serializedObject.FindProperty("DType");
        VType = serializedObject.FindProperty("VType");
        DamageValue = serializedObject.FindProperty("DamageValue");
        Alpha = serializedObject.FindProperty("Alpha");
        PMat = serializedObject.FindProperty("PMat");
        ViewBlock = serializedObject.FindProperty("ViewBlock");
        BG = (BlockGenerator)target;
        Block = BG.transform.gameObject;

        GridBool = new AnimBool(GridFlag);
        GridBool.valueChanged.AddListener(Repaint);
    }

    private void OnSceneGUI()
    {
        
        var result = Handles.FreeMoveHandle(Block.transform.position, Block.transform.rotation, 1f, Vector3.one, Handles.SphereHandleCap);
        if (GridFlag)
        {
            result.x = MultipleRound(result.x, GridSpe);
            result.y = MultipleRound(result.y, GridSpe);
            result.z = MultipleRound(result.z, GridSpe);
        }
        Block.transform.position = result;
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

     //   EditorGUILayout.PropertyField(BlockPos, new GUIContent("ブロックの位置", "ブロックの位置座標の変更、表示を行う"));

        EditorGUILayout.PropertyField(XN, new GUIContent("X方向の連数", "X方向に何個のブロックを連ねるか"));
        EditorGUILayout.PropertyField(YN, new GUIContent("Y方向の連数", "Y方向に何個のブロックを連ねるか"));
        EditorGUILayout.PropertyField(ZN, new GUIContent("Z方向の連数", "Z方向に何個のブロックを連ねるか"));

        EditorGUILayout.PropertyField(Element, new GUIContent("ブロックの属性", "ブロックの属性を設定"));

        if (Element.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(Type, new GUIContent("ブロックの種類", "ブロックにどのテクスチャーを使うか"));
        }
        else if (Element.enumValueIndex == 1)
        {
            EditorGUILayout.PropertyField(DType, new GUIContent("ブロックの種類", "ブロックにどのテクスチャーを使うか"));
            EditorGUILayout.PropertyField(DamageValue, new GUIContent("ダメージ量", "プレイヤーに与えるダメージ量"));
        }
        else if (Element.enumValueIndex == 2)
        {
            EditorGUILayout.PropertyField(VType, new GUIContent("ブロックの種類", "ブロックにどのテクスチャーを使うか"));
            EditorGUILayout.PropertyField(Alpha, new GUIContent("透明度", "ブロックの透明度"));
        }
        EditorGUILayout.PropertyField(PMat, new GUIContent("ブロックの材質", "ブロックの物理特性を設定"));
        EditorGUILayout.PropertyField(ViewBlock, new GUIContent("表示するブロックの領域", "表示するブロックの領域"));

        //  Block.transform.position = BlockPos.vector3Value;
        serializedObject.ApplyModifiedProperties();
    }

    public float MultipleFloor(float value, float multiple)
    {
        return Mathf.Floor(value / multiple) * multiple;
    }

    public float MultipleRound(float value, float multiple)
    {
        return MultipleFloor(value + multiple * 0.5f, multiple);
    }
}
