using UnityEditor;
using UnityEngine;

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
        for (int y = 0; y < stageObject1.GetLength(1); y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < stageObject1.GetLength(0); x++)
            {
                int localX = x;
                int localY = y;
                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.normal.textColor = Color.black;
                style.normal.background = Texture2D.whiteTexture;

                GUI.backgroundColor = GetColor(stageObject1[localX, localY].stageObjectType);

                if (GUILayout.Button(stageObject1[localX, localY].stageObjectType.ToString(), style, GUILayout.Width(50), GUILayout.Height(50)))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (StageObjectType type in System.Enum.GetValues(typeof(StageObjectType)))
                    {
                        StageObjectType localType = type;

                        menu.AddItem(new GUIContent(localType.ToString()), false, () =>
                        {
                            Debug.Log($"[{localX}, {localY}] : 変更前内容{stageObject1[localX, localY].stageObjectType} => 変更後内容{localType}");
                            stageObject1[localX, localY].stageObjectType = localType;
                        });
                    }

                    menu.ShowAsContext();
                }
                if (stageObject1[localX, localY].stageObjectType == StageObjectType.Barrel) GUI.backgroundColor = Color.red;

            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.LabelField("ステージの2マス目");
        for (int y = 0; y < stageObject2.GetLength(1); y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < stageObject2.GetLength(0); x++)
            {
                int localX = x;
                int localY = y;

                if (GUILayout.Button(stageObject2[localX, localY].stageObjectType.ToString(), GUILayout.Width(50), GUILayout.Height(50)))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (StageObjectType type in System.Enum.GetValues(typeof(StageObjectType)))
                    {
                        StageObjectType localType = type;

                        menu.AddItem(new GUIContent(localType.ToString()), false, () =>
                        {
                            Debug.Log($"[{localX}, {localY}] : 変更前内容{stageObject2[localX, localY].stageObjectType} => 変更後内容{localType}");
                            stageObject2[localX, localY].stageObjectType = localType;
                        });
                    }

                    menu.ShowAsContext();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

}
