public class GameManagerStage1to3 : GeneralStageManager
{
    void Start()
    {
        StartData();
    }

    void Update()
    {
        switch (status)
        {
            case GameStatus.start:
                playerController.status = 0;
                StartAnime();
                break;
            case GameStatus.play:
                Gimmick1();
                Gimmick2();
                Gimmick3();
                JudgeOver();
                Goal();
                if (menuFlag) status = GameStatus.menu;
                playerController.status = 1;
                break;
            case GameStatus.stop:
                Gimmick1();
                Gimmick2();
                Gimmick3();
                Goal();
                playerController.status = 2;
                break;
            case GameStatus.menu:
                MenuUIControl();
                if (!menuFlag) status = GameStatus.play;
                playerController.status = 3;
                break;
            case GameStatus.over:
                Over();
                OverUIControl();
                playerController.status = 4;
                break;
            case GameStatus.clear:
                playerController.status = 5;
                EndAnime();
                break;
        }
    }
    // 左下エリアのボタンで右上エリアと左下エリアの回転ギミック
    public void Gimmick1()
    {
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag)
        {
            AreaRotation(areas[1], lightObjects[0], null, -1, 90, 2, 0, false, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
            AreaRotation(areas[2], lightObjects[1], cameraPointObjects[0], -1, 90, 2, 1, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
        }
    }
    // 右下エリアの敵前撃破で開放ギミック
    public void Gimmick2()
    {
        if (enterArea[3].enterAreaFlag) Gate(gateObjects[0], lightObjects[2], cameraPointObjects[1], false, 2, 0, true, ref enterArea[3].enterAreaFlag);
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(areaLightObjects[0], 2, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[0],lightObjects[2], null, true, 2, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[1], lightObjects[3], cameraPointObjects[2], true, 2, 1, true, ref defeatGateFlag[0]);
        }
    }
    // 右上エリアのボタンで右上エリアの回転ギミック
    public void Gimmick3()
    {
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[1], lightObjects[0], cameraPointObjects[3], -1, 180, 2, 0, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
}

