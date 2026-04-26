using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CreateStage))]
public class CreateStageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CreateStage script = (CreateStage)target;

        // ベーススクリプトの変数表示
        DrawDefaultInspector();

        /* ========== ステージマスの実装 ========== */
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.fontSize = 20;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("ステージ構成", labelStyle);
        EditorGUI.indentLevel++;
        EditorGUILayout.Space();
        labelStyle.fontSize = 15;
        labelStyle.alignment = TextAnchor.MiddleLeft;
        EditorGUILayout.LabelField("高さ1マス目", labelStyle);
        EditorGUILayout.Space();
        StageGridsButton(script.stageLowGrids);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("高さ2マス目", labelStyle);
        EditorGUILayout.Space();
        StageGridsButton(script.stageHighGrids);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
        EditorGUI.indentLevel--;

        /* ========== ステージを生成するボタンの実装 ========== */

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ステージを生成", GUILayout.Width(200), GUILayout.Height(25)))
        {
            // ステージ生成の処理
            DisplayGrids();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ステージリセット", GUILayout.Width(200), GUILayout.Height(25)))
        {
            script.InitStageGrids();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }
    /// <summary>
    /// ステージ構成のボタン
    /// </summary>
    /// <param name="grids"></param>
    void StageGridsButton(List<StageObject> grids)
    {
        CreateStage script = (CreateStage)target;

        for (int y = 0; y < script.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int x = 0; x < script.width; x++)
            {
                int indexX = x;
                int indexY = y;

                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.normal.textColor = GetTextColor(grids[indexX + indexY * script.width].type);
                style.fontSize = script.fontSize;
                style.fontStyle = FontStyle.Bold;
                style.wordWrap = true;
                style.normal.background = GetColorTexture(grids[indexX + indexY * script.width].type);

                // ステージのボタン
                if (GUILayout.Button($"{grids[indexX + indexY * script.width].type.ToString()}\n{GetDegreeArrow(grids[indexX + indexY * script.width].degree)}", style, GUILayout.Width(script.buttonSize), GUILayout.Height(script.buttonSize)))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (ObjectType type in System.Enum.GetValues(typeof(ObjectType)))
                    {
                        ObjectType localType = type;
                        // 最初のメニュー
                        menu.AddItem(new GUIContent("オブジェクト選択/" + localType.ToString()), false, () =>
                        {
                            Debug.Log($"[{indexX}, {indexY}] : 変更前内容{grids[indexX + indexY * script.width].type} => 変更後内容{localType}");
                            grids[indexX + indexY * script.width].type = localType;
                            EditorUtility.SetDirty(script);
                        });
                    }

                    foreach (Degree degree in System.Enum.GetValues(typeof(Degree)))
                    {
                        menu.AddItem(new GUIContent("方向選択/" + degree.ToString()), false, () =>
                        {
                            Debug.Log($"[{indexX}, {indexY}] : 変更前内容{grids[indexX + indexY * script.width].degree} => 変更後内容{degree}");
                            grids[indexX + indexY * script.width].degree = (int)degree;
                            EditorUtility.SetDirty(script);
                        });
                    }

                    menu.ShowAsContext();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }

    #region 変換関数群
    // ObjectTypeからマスのTextカラーを取得
    Color GetTextColor(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.None: return Color.black;
            case ObjectType.Barrel: return Color.white;
            case ObjectType.Button: return Color.white;
            case ObjectType.Chest: return Color.black;
            case ObjectType.Gate: return Color.white;
            case ObjectType.Goal: return Color.black;
            case ObjectType.GroundButton: return Color.white;
            case ObjectType.Slope: return Color.black;
            case ObjectType.Start: return Color.black;
            case ObjectType.Wall: return Color.white;
            case ObjectType.Wood: return Color.white;
            default: return Color.black;
        }
    }
    // ObjectTypeからマスのBackGroundカラーを取得
    Texture2D GetColorTexture(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.None: return MakeTex(Color.white);
            case ObjectType.Barrel: return MakeTex(Color.brown);
            case ObjectType.Button: return MakeTex(Color.blue);
            case ObjectType.Chest: return MakeTex(Color.goldenRod);
            case ObjectType.Gate: return MakeTex(Color.gray);
            case ObjectType.Goal: return MakeTex(Color.green);
            case ObjectType.GroundButton: return MakeTex(Color.blueViolet);
            case ObjectType.Slope: return MakeTex(Color.gray8);
            case ObjectType.Start: return MakeTex(Color.yellow);
            case ObjectType.Wall: return MakeTex(Color.black);
            case ObjectType.Wood: return MakeTex(Color.saddleBrown);
            default: return MakeTex(Color.white);
        }
    }
    
    // ColorからTexture2Dを作成
    Texture2D MakeTex(Color col)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }

    string GetDegreeArrow(int degree)
    {
        switch (degree)
        {
            case 0: return "↑";
            case 90: return "→";
            case 180: return "↓";
            case 270: return "←";
            default : return "↑";
        }
    }

    #endregion


    #region ステージ生成関連
    void GenerateStage()
    {
        // 
    }
    #endregion

    void DisplayGrids()
    {
        CreateStage script = (CreateStage)target;
        for (int y = 0; y < script.height; y++)
        {
            for (int x = 0; x < script.width; x++)
            {
                int indexX = x;
                int indexY = y;
                Debug.Log($"[{indexX}, {indexY}]Object : {script.stageLowGrids[indexX + indexY * script.width].type.ToString()}");
            }
        }
    }

}
