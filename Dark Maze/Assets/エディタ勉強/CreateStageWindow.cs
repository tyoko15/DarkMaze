using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;



public class CreateStageWindow : EditorWindow
{

    CreateStage script;
    Vector2 scroll;

    /* StageObject */

    [MenuItem("Window/CreateStageWindow")]
    public static void Open()
    {
        GetWindow<CreateStageWindow>();
    }

    void OnEnable()
    {
        script = FindObjectOfType<CreateStage>();
    }

    bool[] SGfolds = new bool[2];

    // Windowの構成
    void OnGUI()
    {
        if (script == null)
        {
            script = FindObjectOfType<CreateStage>();
            if (script == null) EditorGUILayout.LabelField("CreateStageスクリプトが見つかりません。");
        }
        else
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            /* ========== ステージマスの実装 ========== */
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
            GUIStyle H1Style = SetHeadingText(UIType.Label, Heading.H1);
            H1Style.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("----- ステージ構成 -----", H1Style, GUILayout.Height(H1Style.fontSize));

            /* ---------- 高さ1マス目 ---------- */

            EditorGUI.indentLevel++;
            EditorGUILayout.Space();
            GUIStyle H2Style = SetHeadingText(UIType.Foldout, Heading.H2);
            SGfolds[0] = EditorGUILayout.Foldout(SGfolds[0], "高さ1マス目", H2Style);
            EditorGUILayout.Space();
            if (SGfolds[0]) StageGridsButton(script.stageLowGrids);

            /* ---------- 高さ2マス目 ---------- */

            EditorGUILayout.Space();
            SGfolds[1] = EditorGUILayout.Foldout(SGfolds[1], "高さ2マス目", H2Style);
            EditorGUILayout.Space();
            if (SGfolds[1]) StageGridsButton(script.stageHighGrids);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
            EditorGUI.indentLevel--;

            /* ========== ギミックオブジェクトの内容実装 ========== */

            GimmickGridControl();

            /* ========== ステージを生成するボタンの実装 ========== */

            GUIStyle H1Style3 = SetHeadingText(UIType.Label, Heading.H1);
            H1Style3.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("----- ステージクリエイト -----", H1Style3, GUILayout.Height(H1Style3.fontSize));
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (script.root == null)
            {
                // ステージ生成のボタン
                if (GUILayout.Button("ステージを生成", GUILayout.Width(200), GUILayout.Height(25)))
                {             
                    StageCreate();
                }
                GUILayout.FlexibleSpace();
            }
            else
            {
                // ステージ削除のボタン
                if (GUILayout.Button("ステージを削除", GUILayout.Width(200), GUILayout.Height(25)))
                {
                    Undo.DestroyObjectImmediate(script.root);
                }
                GUILayout.FlexibleSpace();
            }
            // ステージリセットのボタン
            if (GUILayout.Button("ステージリセット", GUILayout.Width(200), GUILayout.Height(25)))
            {
                script.InitStageGrids();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// ステージ構成のボタン
    /// </summary>
    /// <param name="grids"></param>
    void StageGridsButton(List<StageObject> grids)
    {
        for (int y = 0; y < script.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int x = 0; x < script.width; x++)
            {
                int indexX = x;
                int indexY = y;

                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.normal.textColor = CastObjectTypeToTextColor(grids[indexX + indexY * script.width].type);
                style.fontSize = script.fontSize;
                style.fontStyle = FontStyle.Bold;
                style.wordWrap = true;
                //style.normal.background = CastObjectTypeToTexture2D(grids[indexX + indexY * script.width].type);
                GUI.backgroundColor = CastObjectTypeToColor(grids[indexX + indexY * script.width].type);

                // ステージのボタン
                string name = grids[indexX + indexY * script.width].type.ToString();
                if (grids[indexX + indexY * script.width].type == ObjectType.Button) name = $"{script.gimmickArgument.gimmickGridList[grids[indexX + indexY * script.width].gimmickNumber].name}";
                if (GUILayout.Button($"{name}\n{CastDegreeToArrow(grids[indexX + indexY * script.width].degree)}", style, GUILayout.Width(script.buttonSize), GUILayout.Height(script.buttonSize)))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (ObjectType type in System.Enum.GetValues(typeof(ObjectType)))
                    {
                        ObjectType localType = type;
                        // 最初のメニュー
                        menu.AddItem(new GUIContent("オブジェクト選択/" + localType.ToString()), false, () =>
                        {
                            //Debug.Log($"[{indexX}, {indexY}] : 変更前内容{grids[indexX + indexY * script.width].type} => 変更後内容{localType}");
                            ObjectType before = grids[indexX + indexY * script.width].type;
                            ObjectType after = localType;

                            grids[indexX + indexY * script.width].type = localType;
                            bool flag = (grids == script.stageLowGrids) ? false : true;
                            if (before != ObjectType.Button && after == ObjectType.Button) script.IncreaseGimmickFanctionList(grids[indexX + indexY * script.width], flag, localType, new Vector2(indexX, indexY));
                            else if (before == ObjectType.Button && after != ObjectType.Button) script.DecreaseGimmickFanctionList(grids[indexX + indexY * script.width], flag, new Vector2(indexX, indexY));

                            EditorUtility.SetDirty(script);
                        });
                    }

                    foreach (Degree degree in System.Enum.GetValues(typeof(Degree)))
                    {
                        menu.AddItem(new GUIContent("方向選択/" + degree.ToString()), false, () =>
                        {
                            //Debug.Log($"[{indexX}, {indexY}] : 変更前内容{grids[indexX + indexY * script.width].degree} => 変更後内容{degree}");
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
        GUI.backgroundColor = Color.white;
    }

    void GimmickGridControl()
    {
        if (script.gimmickArgument.gimmickGridList.Count == 0) return;
        GUIStyle H1Style2 = SetHeadingText(UIType.Label, Heading.H1);
        H1Style2.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("----- ギミック構成 -----", H1Style2, GUILayout.Height(H1Style2.fontSize));
        EditorGUILayout.Space();

        for (int i = 0; i < script.gimmickArgument.gimmickGridList.Count; i++)
        {
            GimmickObjectFanction(i, script.gimmickArgument.gimmickGridList[i]);
        }
    }

    bool GFfold = true;
    void GimmickObjectFanction(int i, GimmickGrid gimmickGrid)
    {
        GimmickFanction gimmickFanction = gimmickGrid.gimmickFanction;
        EditorGUILayout.LabelField($"{gimmickGrid.name}");
        script.gimmickArgument.gimmickGridList[i].gimmickFanction = (GimmickFanction)EditorGUILayout.EnumPopup("発動関数", script.gimmickArgument.gimmickGridList[i].gimmickFanction);
        EditorGUI.indentLevel++;
        if (gimmickFanction != GimmickFanction.None) GFfold = EditorGUILayout.Foldout(GFfold, "引数設定");
        if (GFfold)
        {

            //switch (gimmickFanction)
            //{
            //    case GimmickFanction.None:
            //        break;
            //    case GimmickFanction.AreaRotation:
            //        script.testAr.area = (GameObject)EditorGUILayout.ObjectField("エリアオブジェクト", script.testAr.area, typeof(GameObject), true);
            //        script.testAr.light = (GameObject)EditorGUILayout.ObjectField("ライトオブジェクト", script.testAr.light, typeof(GameObject), true);
            //        script.testAr.cameraPoint = (GameObject)EditorGUILayout.ObjectField("カメラポイントオブジェクト", script.testAr.cameraPoint, typeof(GameObject), true);
            //        script.testAr.direction = EditorGUILayout.IntField("回転方向", script.testAr.direction);
            //        script.testAr.degree = EditorGUILayout.IntField("回転度", script.testAr.degree);
            //        script.testAr.time = EditorGUILayout.FloatField("タイム", script.testAr.time);
            //        script.testAr.end = EditorGUILayout.Toggle("タイム", script.testAr.end);
            //        script.testAr.flag = EditorGUILayout.Toggle("終了フラグ", script.testAr.flag);
            //        break;
            //    case GimmickFanction.SenceGate:
            //        script.testSg.gate = (GameObject)EditorGUILayout.ObjectField("ゲートオブジェクト", script.testSg.gate, typeof(GameObject), true);
            //        script.testSg.light = (GameObject)EditorGUILayout.ObjectField("ライトオブジェクト", script.testSg.light, typeof(GameObject), true);
            //        script.testSg.cameraPoint = (GameObject)EditorGUILayout.ObjectField("カメラポイントオブジェクト", script.testSg.cameraPoint, typeof(GameObject), true);
            //        script.testSg.open = EditorGUILayout.Toggle("開閉フラグ", script.testSg.open);
            //        script.testSg.complete = EditorGUILayout.Toggle("開閉フラグ", script.testSg.complete);
            //        script.testSg.time = EditorGUILayout.FloatField("タイム", script.testSg.time);
            //        script.testSg.i = EditorGUILayout.IntField("番号", script.testSg.i);
            //        break;
            //    case GimmickFanction.Gate:
            //        script.testG.gate = (GameObject)EditorGUILayout.ObjectField("ゲートオブジェクト", script.testG.gate, typeof(GameObject), true);
            //        script.testG.light = (GameObject)EditorGUILayout.ObjectField("ライトオブジェクト", script.testG.light, typeof(GameObject), true);
            //        script.testG.cameraPoint = (GameObject)EditorGUILayout.ObjectField("カメラポイントオブジェクト", script.testG.cameraPoint, typeof(GameObject), true);
            //        script.testG.open = EditorGUILayout.Toggle("開閉フラグ", script.testG.open);
            //        script.testG.time = EditorGUILayout.FloatField("タイム", script.testG.time);
            //        script.testG.i = EditorGUILayout.IntField("番号", script.testG.i);
            //        script.testG.end = EditorGUILayout.Toggle("終了フラグ", script.testG.end);
            //        script.testG.flag = EditorGUILayout.Toggle("フラグ", script.testG.flag);
            //        break;
            //    case GimmickFanction.LimitActiveObject:
            //        script.testLa.activeOb = (GameObject)EditorGUILayout.ObjectField("出現オブジェクト", script.testLa.activeOb, typeof(GameObject), true);
            //        script.testLa.light = (GameObject)EditorGUILayout.ObjectField("ライトオブジェクト", script.testLa.light, typeof(GameObject), true);
            //        script.testLa.i = EditorGUILayout.IntField("番号", script.testLa.i);
            //        script.testLa.end = EditorGUILayout.Toggle("終了フラグ", script.testLa.end);
            //        script.testLa.flag = EditorGUILayout.Toggle("フラグ", script.testLa.flag);
            //        break;
            //    case GimmickFanction.ActiveObject:
            //        script.testA.activeOb = (GameObject)EditorGUILayout.ObjectField("出現オブジェクト", script.testA.activeOb, typeof(GameObject), true);
            //        script.testA.light = (GameObject)EditorGUILayout.ObjectField("ライトオブジェクト", script.testA.light, typeof(GameObject), true);
            //        script.testA.cameraPoint = (GameObject)EditorGUILayout.ObjectField("カメラオブジェクト", script.testA.cameraPoint, typeof(GameObject), true);
            //        script.testA.time = EditorGUILayout.FloatField("番号", script.testA.time);
            //        script.testA.i = EditorGUILayout.IntField("番号", script.testA.i);
            //        script.testA.end = EditorGUILayout.Toggle("終了フラグ", script.testA.end);
            //        script.testA.flag = EditorGUILayout.Toggle("フラグ", script.testA.flag);
            //        break;
            //    case GimmickFanction.ActiveLight:
            //        //public GameObject lightOb;
            //        //public float time;
            //        //public int i;
            //        //public bool end;
            //        //public bool flag;
            //        script.testAl.lightOb = (GameObject)EditorGUILayout.ObjectField("ライトオブジェクト", script.testAl.lightOb, typeof(GameObject), true);
            //        script.testAl.time = EditorGUILayout.FloatField("番号", script.testAl.time);
            //        script.testAl.i = EditorGUILayout.IntField("番号", script.testAl.i);
            //        script.testAl.end = EditorGUILayout.Toggle("終了フラグ", script.testAl.end);
            //        script.testAl.flag = EditorGUILayout.Toggle("フラグ", script.testAl.flag);
            //        break;
            //}
        }
        EditorGUI.indentLevel--;
    }

    #region 変換関数群

    /// <summary>
    /// ObjectTypeからテキストカラーを決定
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    Color CastObjectTypeToTextColor(ObjectType type)
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

    Color CastObjectTypeToColor(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.None: return Color.snow;
            case ObjectType.Barrel: return Color.brown;
            case ObjectType.Box: return Color.sandyBrown;
            case ObjectType.Button: return Color.blue;
            case ObjectType.Chest: return Color.goldenRod;
            case ObjectType.Gate: return Color.gray;
            case ObjectType.Goal: return Color.green;
            case ObjectType.GroundButton: return Color.blueViolet;
            case ObjectType.Slope: return Color.gray8;
            case ObjectType.Start: return Color.yellow;
            case ObjectType.Wall: return Color.black;
            case ObjectType.Wood: return Color.saddleBrown;
            default: return Color.white;
        }
    }

    /// <summary>
    /// ObjectTypeからマスのBackGroundカラーを決定
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    Texture2D CastObjectTypeToTexture2D(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.None: return MakeColorTexture(Color.white);
            case ObjectType.Barrel: return MakeColorTexture(Color.brown);
            case ObjectType.Box: return MakeColorTexture(Color.sandyBrown);
            case ObjectType.Button: return MakeColorTexture(Color.blue);
            case ObjectType.Chest: return MakeColorTexture(Color.goldenRod);
            case ObjectType.Gate: return MakeColorTexture(Color.gray);
            case ObjectType.Goal: return MakeColorTexture(Color.green);
            case ObjectType.GroundButton: return MakeColorTexture(Color.blueViolet);
            case ObjectType.Slope: return MakeColorTexture(Color.gray8);
            case ObjectType.Start: return MakeColorTexture(Color.yellow);
            case ObjectType.Wall: return MakeColorTexture(Color.black);
            case ObjectType.Wood: return MakeColorTexture(Color.saddleBrown);
            default: return MakeColorTexture(Color.white);
        }
    }

    /// <summary>
    /// ColorからTexture2Dを決定
    /// </summary>
    /// <param name="col">作成する色</param>
    /// <returns></returns>
    Texture2D MakeColorTexture(Color col)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Degreeから矢印へ決定
    /// </summary>
    /// <param name="degree"></param>
    /// <returns></returns>
    string CastDegreeToArrow(int degree)
    {
        switch (degree)
        {
            case 0: return "↑";
            case 90: return "→";
            case 180: return "↓";
            case 270: return "←";
            default: return "↑";
        }
    }

    #endregion 変換関数群

    void DisplayGrids()
    {
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

    /// <summary>
    /// Scene内にStageObjectを生成する関数
    /// </summary>
    void StageCreate()
    {
        /* 空のGameObjectを生成 */
        script.root = new GameObject($"Stage{script.fieldNumber}-{script.stageNumber}Object");

        /* OuterFrameを生成 */
        GameObject outerFrame = null;
        if (script.fieldNumber == 1)
        {
            InstantiateStageObject(16, Vector3.zero, 0f, (obj) =>
            {
                outerFrame = obj;
                outerFrame.name = "OuterFrame";
                outerFrame.transform.parent = script.root.transform;
            });
        }
        else if (script.fieldNumber == 2)
        {
            InstantiateStageObject(17, Vector3.zero, 0f, (obj) =>
            {
                outerFrame = obj;
                outerFrame.name = "OuterFrame";
                outerFrame.transform.parent = script.root.transform;
            });
        }


        /* AreaObjectを生成 */
        for (int i = 0; i < script.area.Length; i++)
        {
            script.area[i] = new GameObject($"Area ({i})");
            script.area[i].transform.parent = script.root.transform;

            script.heightArea[0 + (2 * i)] = new GameObject($"Height (0)");
            script.heightArea[0 + (2 * i)].transform.parent = script.area[i].transform;
            script.heightArea[1 + (2 * i)] = new GameObject($"Height (1)");
            script.heightArea[1 + (2 * i)].transform.parent = script.area[i].transform;
        }
        script.area[0].transform.position = new Vector3(7f, 0f, -7f);
        script.area[1].transform.position = new Vector3(-7f, 0f, -7f);
        script.area[2].transform.position = new Vector3(7f, 0f, 7f);
        script.area[3].transform.position = new Vector3(-7f, 0f, 7f);

        // 床の生成
        int floorNumber = 0;
        if (script.fieldNumber == 1) floorNumber = 8;
        else if (script.fieldNumber == 1) floorNumber = 9;

        script.floor[0] = new GameObject("Floor");
        script.floor[0].transform.parent = script.heightArea[0].transform;
        script.wall[0] = new GameObject("Wall");
        script.wall[0].transform.parent = script.heightArea[0].transform;
        script.wall[1] = new GameObject("Wall");
        script.wall[1].transform.parent = script.heightArea[1].transform;

        // Area (0)の生成
        Vector3 posi;
        for (int h = 0; h < script.height / 2; h++)
        {
            for (int w = 0; w < script.width / 2; w++)
            {
                posi = new Vector3(13f - (w * 2f), 0f, -13f + (h * 2f));

                InstantiateStageObject(floorNumber, posi, 0f, (obj) =>
                {
                    obj.name = "Floor";
                    obj.transform.parent = script.floor[0].transform;
                });

                // 高さ1マス目のオブジェクトの生成
                ObjectType type = script.stageLowGrids[h * script.width + w].type;
                int degree = script.stageLowGrids[h * script.width + w].degree;
                if (type != ObjectType.None)
                {
                    InstantiateStageObject(script.CastObjectTypeToNumber(type), posi, degree, (obj) =>
                    {
                        //obj.name = type.ToString();
                        if ("Wall" != script.stageLowGrids[h * script.width + w].type.ToString()) Debug.Log($"Type:{script.stageLowGrids[h * script.width + w].type} - Name:{script.stageLowGrids[h * script.width + w].type.ToString()}");
                        //obj.transform.eulerAngles = new Vector3(0f, degree, 0f);
                        switch (type)
                        {
                            case ObjectType.Wall: obj.transform.parent = script.wall[0].transform; break;
                            default: obj.transform.parent = script.heightArea[0].transform; break;
                        }
                    });
                }

                posi = new Vector3(13f - (w * 2f), 2f, -13f + (h * 2f));

                // 高さ2マス目のオブジェクトの生成
                type = script.stageHighGrids[h * script.width + w].type;
                degree = script.stageHighGrids[h * script.width + w].degree;
                if (type != ObjectType.None)
                {
                    InstantiateStageObject(script.CastObjectTypeToNumber(type), posi, degree, (obj) =>
                    {
                        //obj.name = type.ToString();
                        //obj.transform.eulerAngles = new Vector3(0f, degree, 0f);
                        switch (type)
                        {
                            case ObjectType.Wall: obj.transform.parent = script.wall[1].transform; break;
                            default: obj.transform.parent = script.area[1].transform; break;
                        }
                    });
                }
            }
        }

        script.floor[1] = new GameObject("Floor");
        script.floor[1].transform.parent = script.heightArea[2].transform;
        script.wall[2] = new GameObject("Wall");
        script.wall[2].transform.parent = script.heightArea[2].transform;
        script.wall[3] = new GameObject("Wall");
        script.wall[3].transform.parent = script.heightArea[3].transform;

        // Area (1)の生成
        for (int h = 0; h < script.height / 2; h++)
        {
            for (int w = 0; w < script.width / 2; w++)
            {
                posi = new Vector3(-1f - (w * 2), 0f, -13f + (h * 2));

                InstantiateStageObject(floorNumber, posi, 0f, (obj) =>
                {
                    obj.name = "Floor";
                    obj.transform.parent = script.floor[1].transform;
                });

                // 高さ1マス目のオブジェクトの生成
                ObjectType type = script.stageLowGrids[h * script.width + (w + script.height / 2)].type;
                int degree = script.stageLowGrids[h * script.width + (w + script.height / 2)].degree;
                if (type != ObjectType.None)
                {
                    InstantiateStageObject(script.CastObjectTypeToNumber(type), posi, degree, (obj) =>
                    {
                        //obj.name = type.ToString();
                        //obj.transform.eulerAngles = new Vector3(0f, degree, 0f);
                        switch (type)
                        {
                            case ObjectType.Wall: obj.transform.parent = script.wall[2].transform; break;
                            default: obj.transform.parent = script.area[1].transform; break;
                        }
                    });
                }

                posi = new Vector3(-1f - (w * 2), 2f, -13f + (h * 2));

                // 高さ2マス目のオブジェクトの生成
                type = script.stageHighGrids[h * script.width + (w + script.height / 2)].type;
                degree = script.stageHighGrids[h * script.width + (w + script.height / 2)].degree;
                if (type != ObjectType.None)
                {
                    InstantiateStageObject(script.CastObjectTypeToNumber(type), posi, degree, (obj) =>
                    {
                        //obj.name = type.ToString();
                        //obj.transform.eulerAngles = new Vector3(0f, degree, 0f);
                        switch (type)
                        {
                            case ObjectType.Wall: obj.transform.parent = script.wall[3].transform; break;
                            default: obj.transform.parent = script.area[1].transform; break;
                        }
                    });
                }
            }
        }

        script.floor[2] = new GameObject("Floor");
        script.floor[2].transform.parent = script.heightArea[4].transform;
        script.wall[4] = new GameObject("Wall");
        script.wall[4].transform.parent = script.heightArea[4].transform;
        script.wall[5] = new GameObject("Wall");
        script.wall[5].transform.parent = script.heightArea[5].transform;

        // Area (2)の生成
        for (int h = 0; h < script.height / 2; h++)
        {
            for (int w = 0; w < script.width / 2; w++)
            {
                posi = new Vector3(13f - (w * 2), 0f, 1f + (h * 2));

                InstantiateStageObject(floorNumber, posi, 0f, (obj) =>
                {
                    obj.name = "Floor";
                    obj.transform.parent = script.floor[2].transform;
                });

                // 高さ1マス目のオブジェクトの生成
                ObjectType type = script.stageLowGrids[(h + script.height / 2) * script.width + w].type;
                int degree = script.stageLowGrids[(h + script.height / 2) * script.width + w].degree;
                if (type != ObjectType.None)
                {
                    InstantiateStageObject(script.CastObjectTypeToNumber(type), posi, degree, (obj) =>
                    {
                        obj.name = type.ToString();
                        //obj.transform.eulerAngles = new Vector3(0f, degree, 0f);
                        switch (type)
                        {
                            case ObjectType.Wall: obj.transform.parent = script.wall[4].transform; break;
                            default: obj.transform.parent = script.area[2].transform; break;
                        }
                    });
                }

                posi = new Vector3(13f - (w * 2), 2f, 1f + (h * 2));

                // 高さ2マス目のオブジェクトの生成
                type = script.stageHighGrids[(h + script.height / 2) * script.width + w].type;
                degree = script.stageHighGrids[(h + script.height / 2) * script.width + w].degree;
                if (type != ObjectType.None)
                {
                    InstantiateStageObject(script.CastObjectTypeToNumber(type), posi, degree, (obj) =>
                    {
                        obj.name = type.ToString();
                        //obj.transform.eulerAngles = new Vector3(0f, degree, 0f);
                        switch (type)
                        {
                            case ObjectType.Wall: obj.transform.parent = script.wall[5].transform; break;
                            default: obj.transform.parent = script.area[2].transform; break;
                        }
                    });
                }
            }
        }

        script.floor[3] = new GameObject("Floor");
        script.floor[3].transform.parent = script.heightArea[6].transform;
        script.wall[6] = new GameObject("Wall");
        script.wall[6].transform.parent = script.heightArea[6].transform;
        script.wall[7] = new GameObject("Wall");
        script.wall[7].transform.parent = script.heightArea[7].transform;

        // Area (3)の生成
        for (int h = 0; h < script.height / 2; h++)
        {
            for (int w = 0; w < script.width / 2; w++)
            {
                posi = new Vector3(-1f - (w * 2), 0f, 1f + (h * 2));

                InstantiateStageObject(floorNumber, posi, 0f, (obj) =>
                {
                    obj.name = "Floor";
                    obj.transform.parent = script.floor[3].transform;
                });

                // 高さ1マス目のオブジェクトの生成
                ObjectType type = script.stageLowGrids[(h + script.height / 2) * script.width + (w + script.width / 2)].type;
                int degree = script.stageLowGrids[(h + script.height / 2) * script.width + (w + script.width / 2)].degree;
                if (type != ObjectType.None)
                {
                    InstantiateStageObject(script.CastObjectTypeToNumber(type), posi, degree, (obj) =>
                    {
                        obj.name = type.ToString();
                        //obj.transform.eulerAngles = new Vector3(0f, degree, 0f);
                        switch (type)
                        {
                            case ObjectType.Wall: obj.transform.parent = script.wall[6].transform; break;
                            default: obj.transform.parent = script.area[3].transform; break;
                        }
                    });
                }

                posi = new Vector3(-1f - (w * 2), 2f, 1f + (h * 2));

                // 高さ2マス目のオブジェクトの生成
                type = script.stageHighGrids[(h + script.height / 2) * script.width + (w + script.width / 2)].type;
                degree = script.stageHighGrids[(h + script.height / 2) * script.width + (w + script.width / 2)].degree;
                if (type != ObjectType.None)
                {
                    InstantiateStageObject(script.CastObjectTypeToNumber(type), posi, degree, (obj) =>
                    {
                        obj.name = type.ToString();
                        //obj.transform.eulerAngles = new Vector3(0f, degree, 0f);
                        switch (type)
                        {
                            case ObjectType.Wall: obj.transform.parent = script.wall[7].transform; break;
                            default: obj.transform.parent = script.area[3].transform; break;
                        }
                    });
                }
            }
        }
    }


    enum Heading
    {
        H1, 
        H2, 
        H3,
        Normal
    }
    public enum UIType
    {
        Label,
        Button,
        Toggle,
        TextField,
        TextArea,
        Foldout,
        Slider,
        IntField,
        FloatField,
        ObjectField,
        EnumPopup,
        Popup,
        Box,
        HelpBox,
        ToolbarButton,
    }
    GUIStyle GetBaseStyle(UIType type)
    {
        return type switch
        {
            UIType.Label => new GUIStyle(EditorStyles.label),
            UIType.Button => new GUIStyle(GUI.skin.button),
            UIType.Toggle => new GUIStyle(EditorStyles.toggle),
            UIType.TextField => new GUIStyle(EditorStyles.textField),
            UIType.TextArea => new GUIStyle(EditorStyles.textArea),
            UIType.Foldout => new GUIStyle(EditorStyles.foldout),
            UIType.Box => new GUIStyle(GUI.skin.box),
            UIType.HelpBox => new GUIStyle(EditorStyles.helpBox),
            UIType.ToolbarButton => new GUIStyle(EditorStyles.toolbarButton),
            _ => new GUIStyle(EditorStyles.label)
        };
    }

    GUIStyle SetHeadingText(UIType type, Heading heading)
    {
        GUIStyle style = GetBaseStyle(type);

        switch (heading)
        {
            case Heading.H1:
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 30;
                break;
            case Heading.H2:
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 25;
                break;
            case Heading.H3:
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 20;
                break;
            case Heading.Normal:
            default:
                style.fontStyle = FontStyle.Normal;
                style.fontSize = 12;
                break;
        }
        return style;
    }


    /// <summary>
    /// Addressablesを使用してStageObjectを生成
    /// </summary>
    /// <param name="i">アドレス番号</param>
    /// <param name="position">生成位置</param>
    /// <param name="onComplete">生成オブジェクト</param>
    void InstantiateStageObject(int i, Vector3 position, float degree, System.Action<GameObject> onComplete)
    {
        Addressables.LoadAssetAsync<GameObject>(script.stageObjectAddress[i]).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = handle.Result;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = position;
                instance.transform.eulerAngles = new Vector3(0f, degree, 0f);

                Undo.RegisterCreatedObjectUndo(instance, "CreateStageObject");
                instance.name = script.stageObjectAddress[i];
                onComplete?.Invoke(instance);
            }
        };
    }
}
