using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemObject))]
public class ItemObjectEditor : Editor
{
    private SerializedProperty itemType;
    private SerializedProperty RecoverAmount;
    private SerializedProperty Price;
    private SerializedProperty ShopNumber;

    private void OnEnable()
    {
        itemType = serializedObject.FindProperty("itemType");
        RecoverAmount = serializedObject.FindProperty("RecoverAmount");
        Price = serializedObject.FindProperty("Price");
        ShopNumber = serializedObject.FindProperty("ShopNumber");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(itemType, new GUIContent("アイテムの種類", "アイテムの種類を選択"));
        if (itemType.enumValueIndex == 0 || itemType.enumValueIndex == 1)
        {
            EditorGUILayout.PropertyField(RecoverAmount, new GUIContent("回復量", "プレイヤーが回復する量"));
        }

        if (itemType.enumValueIndex == 3)
        {
            EditorGUILayout.PropertyField(ShopNumber, new GUIContent("商品ナンバー", "商品ナンバー"));
            EditorGUILayout.PropertyField(Price, new GUIContent("価値", "購入に必要なコインの枚数"));
        }

        serializedObject.ApplyModifiedProperties();
    }

}
