using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(MoveObject2))]
public class MoveObjectEditor2 : Editor
{
    private AnimBool MoveBool;
    private AnimBool RotBool;
    private AnimBool SwitchBool;
    private AnimBool OnOffBool;
    private AnimBool LoopBool;

    private SerializedProperty SingleFalg;

    private SerializedProperty MoveFlag;
    private SerializedProperty MoveReversFlag;
    private SerializedProperty MoveVector;
    private SerializedProperty MoveTime;
    private SerializedProperty MoveInterval;
    private SerializedProperty FirstMove;

    private SerializedProperty RotationFlag;
    private SerializedProperty RotReversFlag;
    private SerializedProperty RotateSita;
    private SerializedProperty RotateTime;
    private SerializedProperty RotateInterval;
    private SerializedProperty FirstRot;

    private SerializedProperty OnOffFlag;
    private SerializedProperty OnOffLoop;
    private SerializedProperty LoopOnStart;
    private SerializedProperty OnTime;
    private SerializedProperty OffTime;
    private SerializedProperty LagTime;
    private SerializedProperty FirstOnOff;

    private SerializedProperty SwitchFlag;
    private SerializedProperty SwitchNumbers;
    private int SwitchCount;

    private SerializedProperty MoveViewBlock;
    private SerializedProperty MoveVecterObj;
  //  private SerializedProperty RotVecterObj;

    private MoveObject2 moveObj;
    private GameObject Block = null;
    private GameObject moveBlock = null;
    private GameObject moveVector = null;
 //   private GameObject rotVector = null;

   // private BlockGenerator BlockGene = null;
    private GameObject ViewBlock = null;
    
    private void OnEnable()
    {
        SingleFalg = serializedObject.FindProperty("SingleFalg");

        MoveFlag = serializedObject.FindProperty("MoveFlag");
        MoveReversFlag = serializedObject.FindProperty("MoveReversFlag");
        MoveVector = serializedObject.FindProperty("MoveVector");
        MoveTime = serializedObject.FindProperty("MoveTime");
        MoveInterval = serializedObject.FindProperty("MoveInterval");
        FirstMove = serializedObject.FindProperty("FirstMove");

        RotationFlag = serializedObject.FindProperty("RotationFlag");
        RotReversFlag = serializedObject.FindProperty("RotReversFlag");
        RotateSita = serializedObject.FindProperty("RotateSita");
        RotateTime = serializedObject.FindProperty("RotateTime");
        RotateInterval = serializedObject.FindProperty("RotateInterval");
        FirstRot = serializedObject.FindProperty("FirstRot");

        OnOffFlag = serializedObject.FindProperty("OnOffFlag");
        OnOffLoop = serializedObject.FindProperty("OnOffLoop");
        LoopOnStart = serializedObject.FindProperty("LoopOnStart");
        OnTime = serializedObject.FindProperty("OnTime");
        OffTime = serializedObject.FindProperty("OffTime");
        LagTime = serializedObject.FindProperty("LagTime");
        FirstOnOff = serializedObject.FindProperty("FirstOnOff");

        SwitchFlag = serializedObject.FindProperty("SwitchFlag");
        SwitchNumbers = serializedObject.FindProperty("SwitchNumbers");

        MoveViewBlock = serializedObject.FindProperty("MoveViewBlock");
        MoveVecterObj = serializedObject.FindProperty("MoveVecterObj");

        MoveBool = new AnimBool(MoveFlag.boolValue);
        MoveBool.valueChanged.AddListener(Repaint);
        RotBool = new AnimBool(RotationFlag.boolValue);
        RotBool.valueChanged.AddListener(Repaint);
        OnOffBool = new AnimBool(OnOffFlag.boolValue);
        OnOffBool.valueChanged.AddListener(Repaint);
        SwitchBool = new AnimBool(SwitchFlag.boolValue);
        SwitchBool.valueChanged.AddListener(Repaint);
        LoopBool = new AnimBool(OnOffLoop.boolValue);
        LoopBool.valueChanged.AddListener(Repaint);


        moveObj = (MoveObject2)target;
        Block = moveObj.transform.gameObject;
        if (moveObj.MoveViewBlock != null) moveBlock = moveObj.MoveViewBlock;
        if (moveObj.MoveVecterObj != null) moveVector = moveObj.MoveVecterObj;
        BlockGenerator BGnen = Block.GetComponent<BlockGenerator>();
        if (BGnen != null) ViewBlock = BGnen.GetViewBlock();
        SpringBlockCon SBCon = Block.GetComponent<SpringBlockCon>();
        if (SBCon != null) ViewBlock = SBCon.GetViewBlock();
        
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(SingleFalg, new GUIContent("単体使用", "複数のMoveObjectスクリプトを併用するか"));
        MoveFlag.boolValue = MoveBool.target;
        MoveBool.target = EditorGUILayout.ToggleLeft("移動アクション", MoveBool.target);
        if (EditorGUILayout.BeginFadeGroup(MoveBool.faded) )
        {
            EditorGUILayout.PropertyField(MoveReversFlag, new GUIContent("逆移動あり", "逆移動を含めるか"));
            EditorGUILayout.PropertyField(MoveVector, new GUIContent("移動量", "一回あたりの移動量"));
            EditorGUILayout.PropertyField(MoveTime, new GUIContent("移動時間", "一回の移動に要する時間"));
            EditorGUILayout.PropertyField(MoveInterval, new GUIContent("待機時間", "次の移動までの停止時間"));
            EditorGUILayout.PropertyField(FirstMove, new GUIContent("開始ON状態", "初期状態から稼働させるか"));
        }
        EditorGUILayout.EndFadeGroup();

        RotationFlag.boolValue = RotBool.target;
        RotBool.target = EditorGUILayout.ToggleLeft("回転アクション", RotBool.target);
        if (EditorGUILayout.BeginFadeGroup(RotBool.faded))
        {
            EditorGUILayout.PropertyField(RotReversFlag, new GUIContent("逆回転あり", "逆回転を含めるか"));
            EditorGUILayout.PropertyField(RotateSita, new GUIContent("回転量", "一回あたりの回転量"));
            EditorGUILayout.PropertyField(RotateTime, new GUIContent("回転時間", "一回の回転に要する時間"));
            EditorGUILayout.PropertyField(RotateInterval, new GUIContent("待機時間", "次の回転までの停止時間"));
            EditorGUILayout.PropertyField(FirstRot, new GUIContent("開始ON状態", "初期状態から稼働させるか"));
        }
        EditorGUILayout.EndFadeGroup();

        OnOffFlag.boolValue = OnOffBool.target;
        OnOffBool.target = EditorGUILayout.ToggleLeft("ON・OFFアクション", OnOffBool.target);
        if (EditorGUILayout.BeginFadeGroup(OnOffBool.faded))
        {
            OnOffLoop.boolValue = LoopBool.target;
            LoopBool.target = EditorGUILayout.ToggleLeft(new GUIContent("繰り返し", "On,Offを繰り返すか"), LoopBool.target);
            if (EditorGUILayout.BeginFadeGroup(LoopBool.faded))
            {
                EditorGUILayout.PropertyField(LoopOnStart, new GUIContent("ループ開始時ON状態", "On状態から始めるか"));
                EditorGUILayout.PropertyField(OnTime, new GUIContent("出現時間", "出現している時間"));
                EditorGUILayout.PropertyField(OffTime, new GUIContent("消失時間", "消失している時間"));
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.PropertyField(LagTime, new GUIContent("ラグ", "開始時間のズレ"));
            EditorGUILayout.PropertyField(FirstOnOff, new GUIContent("開始ON状態", "初期状態から稼働させるか"));
        }
        EditorGUILayout.EndFadeGroup();

        // SwitchFlag.boolValue = EditorGUILayout.ToggleLeft("スイッチ使用", SwitchFlag.boolValue);
        SwitchFlag.boolValue = SwitchBool.target;
        SwitchBool.target = EditorGUILayout.ToggleLeft("スイッチ使用", SwitchBool.target);
        if (EditorGUILayout.BeginFadeGroup(SwitchBool.faded))
        {
            EditorGUILayout.PropertyField(SwitchNumbers, new GUIContent("スイッチナンバー", "アクションを起動、停止させる番号"), true);
        }
        EditorGUILayout.EndFadeGroup();

        EditorGUILayout.PropertyField(MoveViewBlock, new GUIContent("移動先のブロック"));
        EditorGUILayout.PropertyField(MoveVecterObj, new GUIContent("移動方向の矢印"));

        //移動ブロックの表示処理
        if (moveBlock != null)
        {
            if (MoveFlag.boolValue || RotationFlag.boolValue)
            {
                if (!moveBlock.activeSelf) moveBlock.SetActive(true);
                Vector3 BlockPos;
                Vector3 BlockScale;
                if (ViewBlock != null)
                {
                    if (MoveFlag.boolValue) BlockPos = ViewBlock.transform.position + MoveVector.vector3Value; else BlockPos = ViewBlock.transform.position;
                    BlockScale = ViewBlock.transform.localScale;
                }
                else
                {
                    if (MoveFlag.boolValue) BlockPos = Block.transform.position + MoveVector.vector3Value; else BlockPos = Block.transform.position;
                    BlockScale = Vector3.one;
                }
                moveBlock.transform.position = BlockPos;
               // moveBlock.transform.localScale = BlockScale;
                moveBlock.transform.eulerAngles = Vector3.zero;
            
                moveBlock.transform.localScale = new Vector3(1 / Block.transform.localScale.x, 1 / Block.transform.localScale.y, 1 / Block.transform.localScale.z);
                Transform ChirdBlock = moveBlock.transform.GetChild(0);
                ChirdBlock.localScale = new Vector3(BlockScale.x * Block.transform.localScale.x, BlockScale.y * Block.transform.localScale.y, BlockScale.z * Block.transform.localScale.z);


                if (RotationFlag.boolValue)
                {
                  //  moveBlock.transform.eulerAngles = Block.transform.eulerAngles + RotateSita.vector3Value;
                    ChirdBlock.eulerAngles = Block.transform.eulerAngles + RotateSita.vector3Value;
                }
                else
                {
                 //   moveBlock.transform.eulerAngles = Block.transform.eulerAngles;
                    ChirdBlock.eulerAngles = Block.transform.eulerAngles;
                }
            }
            else
            {
                if (moveBlock.activeSelf) moveBlock.SetActive(false);
            }
        }

        //移動矢印の表示処理
        if (moveVector != null)
        {
            if (MoveFlag.boolValue && MoveVector.vector3Value != Vector3.zero)
            {
                if (!moveVector.activeSelf) moveVector.SetActive(true);
                if (ViewBlock != null) moveVector.transform.position = ViewBlock.transform.position + MoveVector.vector3Value / 2.0f;
                else moveVector.transform.position = Block.transform.position + MoveVector.vector3Value / 2.0f;

                moveVector.transform.parent = Block.transform;
                moveVector.transform.localScale = new Vector3(1 / Block.transform.localScale.x, 1 / Block.transform.localScale.y, 1 / Block.transform.localScale.z);
                Transform ChirdVec = moveVector.transform.GetChild(0);
                Vector3 AScale = new Vector3(1, 1, MoveVector.vector3Value.magnitude);
                ChirdVec.localScale = AScale;
                ChirdVec.rotation = Quaternion.LookRotation(MoveVector.vector3Value);
            }
            else
            {
                if (moveVector.activeSelf) moveVector.SetActive(false);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
