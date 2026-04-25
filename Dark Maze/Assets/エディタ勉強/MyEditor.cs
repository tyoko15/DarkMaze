using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public enum StageObjectType
{
    None,
    Barrel,
    Button,
    Chest,
    Gate,
    Goal,
    GroundButton,
    Slope,
    Start,
    Wall,
    Wood
}


struct StageObject
{
    public StageObjectType stageObjectType;
    public Color color;
}


[CustomEditor(typeof(Test))]
public class MyEditor : Editor
{
    bool flag;
    int intValue;
    float floatValue;
    float value;
    string text;
    StageObjectType type;
    GameObject obj;
    Color color;
    bool fold;
    Vector2 scroll;

    StageObject[,] stageObject1 = new StageObject[14, 14];
    StageObject[,] stageObject2 = new StageObject[14, 14];

    Color GetColor(StageObjectType type)
    {
        switch (type)
        {
            case StageObjectType.None: return Color.white;
            case StageObjectType.Barrel: return Color.brown;
            case StageObjectType.Button: return Color.blue;
            case StageObjectType.Chest: return Color.goldenRod;
            case StageObjectType.Gate: return Color.gray;
            case StageObjectType.Goal: return Color.green;
            case StageObjectType.GroundButton: return Color.blueViolet;
            case StageObjectType.Slope: return Color.gray5;
            case StageObjectType.Start: return Color.yellow;
            case StageObjectType.Wall: return Color.black;
            case StageObjectType.Wood: return Color.saddleBrown;
            default: return Color.white;
        }
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("実行"))
        {
            Debug.Log("押された！");
        }
        EditorGUILayout.LabelField("ステージの1マス目");
        StageObjectList(stageObject1);
        EditorGUILayout.LabelField("ステージの2マス目");
        StageObjectList(stageObject2);

        //EditorGUILayout.LabelField("ラベル"); 
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("左", "右"); // 2列表示

        flag = EditorGUILayout.Toggle("フラグ", flag);

        intValue = EditorGUILayout.IntField("整数", intValue);
        floatValue = EditorGUILayout.FloatField("小数", floatValue);

        value = EditorGUILayout.Slider("スライダー", value, 0f, 10f);

        text = EditorGUILayout.TextField("文字列", text);

        type = (StageObjectType)EditorGUILayout.EnumPopup("タイプ", type);

        obj = (GameObject)EditorGUILayout.ObjectField("オブジェクト", obj, typeof(GameObject), true);

        color = EditorGUILayout.ColorField("色", color);

        fold = EditorGUILayout.Foldout(fold, "詳細");

        if (fold)
        {
            EditorGUILayout.LabelField("中身");
        }

        EditorGUILayout.HelpBox("説明文", MessageType.Info);

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("ネスト");
        EditorGUI.indentLevel--;

        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < 20; i++)
        {
            GUILayout.Label("項目 " + i);
        }

        EditorGUILayout.EndScrollView();

        EditorGUI.BeginChangeCheck();

        value = EditorGUILayout.FloatField("値", value);

        if (EditorGUI.EndChangeCheck())
        {
            Debug.Log("変更された");
        }

        EditorUtility.SetDirty(this);

        Undo.RecordObject(this, "変更名");
        value = EditorGUILayout.FloatField("値", value);


        if (GUILayout.Button("メニューを開く"))
        {
            GenericMenu menu = new GenericMenu();

            // 通常項目
            menu.AddItem(new GUIContent("項目A"), false, () => Debug.Log("A"));

            // サブメニュー
            menu.AddItem(new GUIContent("カテゴリ/項目B1"), false, () => Debug.Log("B1"));
            menu.AddItem(new GUIContent("カテゴリ/項目B2"), false, () => Debug.Log("B2"));

            // さらに深い階層もOK
            menu.AddItem(new GUIContent("カテゴリ/サブカテゴリ/項目C"), false, () => Debug.Log("C"));

            menu.ShowAsContext(); // 表示
        }
    }

    void StageObjectList(StageObject[,] stageObject)
    {
        for (int y = 0; y < stageObject.GetLength(1); y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < stageObject.GetLength(0); x++)
            {
                int localX = x;
                int localY = y;
                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.normal.textColor = Color.black;
                style.normal.background = Texture2D.whiteTexture;

                GUI.backgroundColor = GetColor(stageObject[localX, localY].stageObjectType);

                if (GUILayout.Button(stageObject[localX, localY].stageObjectType.ToString(), style, GUILayout.Width(50), GUILayout.Height(50)))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (StageObjectType type in System.Enum.GetValues(typeof(StageObjectType)))
                    {
                        StageObjectType localType = type;

                        menu.AddItem(new GUIContent(localType.ToString()), false, () =>
                        {
                            Debug.Log($"[{localX}, {localY}] : 変更前内容{stageObject[localX, localY].stageObjectType} => 変更後内容{localType}");
                            stageObject[localX, localY].stageObjectType = localType;
                        });
                    }

                    menu.ShowAsContext();
                }
                if (stageObject[localX, localY].stageObjectType == StageObjectType.Barrel) GUI.backgroundColor = Color.red;

            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
