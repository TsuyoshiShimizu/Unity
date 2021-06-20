using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;


[CustomEditor(typeof(EnemyGenerator))]
public class EnemyGeneratorEditor : Editor
{
    private SerializedProperty enemyType;               //召喚する敵
    private SerializedProperty Viewflag;                //召喚クリスタルが見えるか
    private SerializedProperty SwitchNumber;            //スイッチでアクションを起動させる
    private SerializedProperty enemyMode;               //敵のモード
    private SerializedProperty activeType;              //敵のアクティブモード
    private SerializedProperty direction;               //敵の初期の向き（2D限定）
    private SerializedProperty Level;                   //敵のレベル
    private SerializedProperty MaxEeneyNumber;          //敵の最大出現数
    private SerializedProperty SameEnemyNumber;         //敵が同時に何体存在できるか
    private SerializedProperty NextInterval;            //次の敵が出現するまでの時間
    private SerializedProperty AddDis;                  //敵を出現させる距離
    private SerializedProperty DeleteDis;               //敵の活動限界距離
    private SerializedProperty EnemyPrefabs;
    private SerializedProperty Effects;

    private AnimBool SwitchBool;
    private AnimBool Mode2DBool;

    private void OnEnable()
    {
        enemyType = serializedObject.FindProperty("enemyType");
        Viewflag = serializedObject.FindProperty("Viewflag");
        SwitchNumber = serializedObject.FindProperty("SwitchNumber");
        enemyMode = serializedObject.FindProperty("enemyMode");
        activeType = serializedObject.FindProperty("activeType");
        direction = serializedObject.FindProperty("direction");
        Level = serializedObject.FindProperty("Level");
        MaxEeneyNumber = serializedObject.FindProperty("MaxEeneyNumber");
        SameEnemyNumber = serializedObject.FindProperty("SameEnemyNumber");
        NextInterval = serializedObject.FindProperty("NextInterval");
        AddDis = serializedObject.FindProperty("AddDis");
        DeleteDis = serializedObject.FindProperty("DeleteDis");
        EnemyPrefabs = serializedObject.FindProperty("EnemyPrefabs");
        Effects = serializedObject.FindProperty("EffectObj");

        SwitchBool = new AnimBool(Viewflag.boolValue);
        SwitchBool.valueChanged.AddListener(Repaint);
        if(enemyMode.enumValueIndex == 1) Mode2DBool = new AnimBool(true); else Mode2DBool = new AnimBool(false);
        Mode2DBool.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(enemyType, new GUIContent("敵の種類", "出現させる敵のプレハブをセットする"));
        EditorGUILayout.PropertyField(Level, new GUIContent("敵のレベル", "出現させる敵のレベル"));

        EditorGUILayout.PropertyField(enemyMode, new GUIContent("敵のモード", "敵が3d,2dどちらで動くか"));
        EditorGUILayout.PropertyField(activeType, new GUIContent("敵のアクティブモード", "敵の行動制限"));
        if (enemyMode.enumValueIndex == 1) Mode2DBool.target = true; else Mode2DBool.target = false;
        if (EditorGUILayout.BeginFadeGroup(Mode2DBool.faded))
        {
            EditorGUILayout.PropertyField(direction, new GUIContent("敵の出現方向", "敵が出現する時の初期方向"));
        }
        EditorGUILayout.EndFadeGroup();

        EditorGUILayout.PropertyField(MaxEeneyNumber, new GUIContent("敵の出現数", "敵が最大何体まで出現するか"));
        EditorGUILayout.PropertyField(SameEnemyNumber, new GUIContent("敵の最大同時出現数", "敵が同時に出現できる数"));
        EditorGUILayout.PropertyField(NextInterval, new GUIContent("出現インターバル", "次の敵を出現さえるまでの間隔"));
        EditorGUILayout.PropertyField(AddDis, new GUIContent("出現感知距離", "プレイヤーがどの程度近づけば敵を出現させるか"));
        EditorGUILayout.PropertyField(DeleteDis, new GUIContent("活動限界距離", "敵がどの範囲まで移動可能か"));

        Viewflag.boolValue = SwitchBool.target;
        SwitchBool.target = EditorGUILayout.ToggleLeft("起動スイッチ使用", SwitchBool.target);
        if (EditorGUILayout.BeginFadeGroup(SwitchBool.faded))
        {
            EditorGUILayout.PropertyField(SwitchNumber, new GUIContent("スイッチナンバー", "アクションを起動、停止させる番号"));
        }
        EditorGUILayout.EndFadeGroup();

        EditorGUILayout.PropertyField(EnemyPrefabs, new GUIContent("敵の登録プレハブ", "召喚器から出現させるプレハブを登録"), true);
        EditorGUILayout.PropertyField(Effects, new GUIContent("エフェクト", "召喚器に使用するエフェクト"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
