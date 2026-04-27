using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
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

public enum Degree
{
    Deg0 = 0,
    Deg90 = 90,
    Deg180 = 180,
    Deg270 = 270
}



[System.Serializable]
public class StageObject
{
    [Header("ステージマスの情報")]
    public ObjectType type;
    public int degree;
}

public class CreateStage : MonoBehaviour
{
    [Header("ステージナンバー")]
    public int fieldNumber;
    public int stageNumber;
    [Header("ステージのマス数")]
    public int width = 14;
    public int height = 14;
    [Header("マスボタンのサイズ")]
    public int buttonSize = 50;
    public int fontSize = 10;
    public List<StageObject> stageLowGrids;
    public List<StageObject> stageHighGrids;

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
    }

    public void GetGimmickFanction_Argument(GimmickFanction fanc)
    {
        switch (fanc)
        {
            case GimmickFanction.AreaRotation: break;

        }
    }
}
