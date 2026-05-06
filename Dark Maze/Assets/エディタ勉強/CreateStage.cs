using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージオブジェクトの種類
/// </summary>
public enum ObjectType
{
    None,
    Barrel,
    Box,
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

/// <summary>
/// ステージオブジェクトの向き
/// </summary>
public enum Degree
{
    Deg0 = 0,
    Deg90 = 90,
    Deg180 = 180,
    Deg270 = 270
}

/// <summary>
/// ギミック関数
/// </summary>　
public enum GimmickFanction
{
    None,
    AreaRotation,
    SenceGate,
    Gate,
    LimitActiveObject,
    ActiveObject,
    ActiveLight
}

#region ギミック関数の引数クラス群

[HideInInspector, System.Serializable]
public class ARArgument
{
    public GameObject area;
    public GameObject light;
    public GameObject cameraPoint;
    public int direction;
    public int degree;
    public float time;
    public bool end;
    public bool flag;
}

[HideInInspector, System.Serializable]
public class SGArgument
{
    public GameObject gate;
    public GameObject light;
    public GameObject cameraPoint;
    public bool open;
    public bool complete;
    public float time;
    public int i;
}

[HideInInspector, System.Serializable]
public class GArgument
{
    public GameObject gate;
    public GameObject light;
    public GameObject cameraPoint;
    public bool open;
    public float time;
    public int i;
    public bool end;
    public bool flag;
}

[HideInInspector, System.Serializable]
public class LAArgument
{
    public GameObject activeOb;
    public GameObject light;
    public int i;
    public bool end;
    public bool flag;  
}

[HideInInspector, System.Serializable]
public class AArgument
{
    public GameObject activeOb;
    public GameObject light;
    public GameObject cameraPoint;
    public float time;
    public int i;
    public bool end;
    public bool flag;
}

[HideInInspector, System.Serializable]
public class ALArgument
{
    public GameObject lightOb;
    public float time;
    public int i;
    public bool end;
    public bool flag;
}

#endregion ギミック関数の引数クラス群

[HideInInspector, System.Serializable]
public class GimmickGrid
{
    public string name;
    public Vector2 position;
    public GimmickFanction gimmickFanction;
}

[HideInInspector, System.Serializable]
public class StageObject
{
    [Header("ステージマスの情報")]
    public ObjectType type;
    public int degree;
    public int gimmickNumber;
}

public class CreateStage : MonoBehaviour
{
    [Header("ステージナンバー")]
    public int fieldNumber = 1;
    public int stageNumber = 1;
    [Header("ステージのマス数")]
    public int width = 14;
    public int height = 14;
    [Header("マスボタンのサイズ")]
    public int buttonSize = 50;
    public int fontSize = 10;
    public List<StageObject> stageLowGrids;
    public List<StageObject> stageHighGrids;

    /* 本番用 */
    [System.Serializable]
    public class GimmickArgument
    {
        public int[] gfNumbers;
        public List<GimmickGrid> gimmickGridList;
        public List<GimmickFanction> gfList;
        public List<ARArgument> arList;
        public List<SGArgument> sgList;
        public List<GArgument> gList;
        public List<LAArgument> laList;
        public List<AArgument> aList;
        public List<ALArgument> alList;
    }
    [HideInInspector] public GimmickArgument gimmickArgument;

    public string[] stageObjectAddress =
    {
        "AreaLight",            // 0
        "ArrowSign",            // 1
        "Barrel",               // 2
        "Box",                  // 3
        "Button",               // 4
        "CameraPosint",         // 5
        "Chest",                // 6
        "EnterArea",            // 7
        "Floor_1",              // 8
        "Floor_2",              // 9
        "Gate",                 // 10
        "GoalObject",           // 11
        "Ground_1 (Rotate)",    // 12
        "Ground_2 (Rotate)",    // 13
        "GroundButton",         // 14
        "LightObject",          // 15
        "OuterFrame_1",         // 16
        "OuterFrame_2",         // 17
        "RespawnPoint",         // 18
        "RopeSign",             // 19
        "Slope_1",              // 20
        "Slope_2",              // 21
        "StartObject",          // 22
        "Wall_1",               // 23
        "Wall_2",               // 24
        "Wood"                  // 25
    };
    
    public GameObject root;
    [HideInInspector] public GameObject[] area = new GameObject[4];
    [HideInInspector] public GameObject[] heightArea = new GameObject[4 * 2];
    [HideInInspector] public GameObject[] floor = new GameObject[4];
    [HideInInspector] public GameObject[] wall = new GameObject[4 * 2];

    private void Reset()
    {
        InitStageGrids();
    }
 

    public void InitStageGrids()
    {
        stageLowGrids.Clear();
        stageHighGrids.Clear();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int indexX = x;
                int indexY = y;

                stageLowGrids.Add(new StageObject());
                stageHighGrids.Add(new StageObject());
                
                if (x == 0 || x == 6 || x == 7 || x == 13 || y == 0 || y == 6 || y == 7 || y == 13)
                {
                    stageLowGrids[indexX + indexY * width].type = ObjectType.Wall;
                    stageHighGrids[indexX + indexY * width].type = ObjectType.Wall;
                }
            }
        }
        gimmickArgument.gimmickGridList.Clear();
        Destroy(root);
    }

    public void IncreaseGimmickFanctionList(StageObject stageGrid, bool low, ObjectType type, Vector2 grid)
    {
        switch (type)
        {
            case ObjectType.Button:
                gimmickArgument.gimmickGridList.Add(new GimmickGrid());
                if (low) stageHighGrids[(int)grid.x + (int)grid.y * width].gimmickNumber = gimmickArgument.gimmickGridList.Count - 1;
                else stageLowGrids[(int)grid.x + (int)grid.y * width].gimmickNumber = gimmickArgument.gimmickGridList.Count - 1;
                gimmickArgument.gimmickGridList[gimmickArgument.gimmickGridList.Count - 1].name = $"B{gimmickArgument.gimmickGridList.Count - 1}";
                gimmickArgument.gimmickGridList[gimmickArgument.gimmickGridList.Count - 1].position = grid;
                break;
            default:
                break;
        }
    }

    public void DecreaseGimmickFanctionList(StageObject stageGrid, bool low, Vector2 grid)
    {
        gimmickArgument.gimmickGridList.RemoveAt(stageGrid.gimmickNumber);
        if (low) stageLowGrids[(int)grid.x + (int)grid.y * width].gimmickNumber = 0;
        else stageHighGrids[(int)grid.x + (int)grid.y * width].gimmickNumber = 0;
    }

    public void DistroyStage()
    {
        Destroy(root);
    }

    public int CastObjectTypeToNumber(ObjectType type)
    {
        int number = 0;
        switch (type)
        {
            case ObjectType.Barrel: return number = 2;
            case ObjectType.Box: return number = 3;
            case ObjectType.Button: return number = 4;
            case ObjectType.Chest: return number = 6;
            case ObjectType.Gate: return number = 10;
            case ObjectType.Goal: return number = 11;
            case ObjectType.GroundButton: return number = 14;
            case ObjectType.Slope: 
                if (fieldNumber == 1) return number = 20;
                else if (fieldNumber == 2) return number = 21;
                break;
            case ObjectType.Start: return number = 22;
            case ObjectType.Wall:
                if (fieldNumber == 1) return number = 23;
                else if (fieldNumber == 2) return number = 24;
                break;
            case ObjectType.Wood: return number = 25;            
        }
        return number;
    }

    public string CastObjectTypeToName(ObjectType type)
    {
        type.ToString();
        string name = type.ToString();
        ////None,
        ////Barrel,
        ////Box,
        ////Button,
        ////Chest,
        ////Gate,
        ////Goal,
        ////GroundButton,
        ////Slope,
        ////Start,
        ////Wall,
        ////Wood
        //switch (type)
        //{
        //    case ObjectType.Barrel: return name = "Barrel";
        //    case ObjectType.Box: return name = "Box";
        //    case ObjectType.Button: return name = "Barrel";
        //    case ObjectType.Chest: return name = "Barrel";
        //    case ObjectType.Gate: return name = "Barrel";
        //    case ObjectType.Goal: return name = "Barrel";
        //    case ObjectType.GroundButton: return name = "Barrel";
        //    case ObjectType.Slope: return name = "Barrel";
        //    case ObjectType.Start: return name = "Barrel";
        //    case ObjectType.Wall: return name = "Barrel";
        //    case ObjectType.Wood: return name = "Barrel";
        //}
        return name;
    }
}
