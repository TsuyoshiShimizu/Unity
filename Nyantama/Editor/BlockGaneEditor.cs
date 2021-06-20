using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(BlockGenerator))]
public class BlockGaneEditor : Editor
{
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
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
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
