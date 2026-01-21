public class GameManagerStage1to4 : GeneralStageManager
{
    bool bothflag = true;
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
                Gimmick4();
                JudgeOver();
                Goal();
                if (menuFlag) status = GameStatus.menu;
                playerController.status = 1;
                break;
            case GameStatus.stop:
                Gimmick1();
                Gimmick2();
                Gimmick3();
                Gimmick4();
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
                playerController.status = 4;
                break;
            case GameStatus.clear:
                playerController.status = 5;
                EndAnime();
                break;
        }
    }

    // 左上エリアの敵撃破でボタン出現ギミック
    // ボタンで左上エリア回転ギミック
    public void Gimmick1()
    {
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(areaLightObjects[0], 2, 0, false, ref defeatGateFlag[0]);
            ActiveObject(buttonObjects[0], lightObjects[0], cameraPointObjects[0], 2, 0, true, ref defeatGateFlag[0]);
        }
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[0], null, cameraPointObjects[1], -1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }
    // 左下エリアと右上エリアの敵撃破でボタン出現ギミック
    // 
    public void Gimmick2()
    {
        if (enterArea[2].enterAreaFlag) Gate(gateObjects[0], lightObjects[1], cameraPointObjects[2], false, 2, 0, true, ref enterArea[2].enterAreaFlag);
        if (enemys[2].transform.childCount == 0 && defeatGateFlag[2])
        {
            ActiveLight(areaLightObjects[2], 2, 2, false, ref defeatGateFlag[2]);
            Gate(gateObjects[0], lightObjects[1], cameraPointObjects[2], true, 2, 0, true, ref defeatGateFlag[2]);
        }
        if (enterArea[1].enterAreaFlag) Gate(gateObjects[1], lightObjects[2], cameraPointObjects[3], false, 2, 1, true, ref enterArea[1].enterAreaFlag);
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            ActiveLight(areaLightObjects[1], 2, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[1], null, null, true, 2, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[2], lightObjects[3], cameraPointObjects[4], true, 2, 2, true, ref defeatGateFlag[1]);
        }
    }
    // 
    public void Gimmick3()
    {
        if (!defeatGateFlag[2] && !defeatGateFlag[1] && bothflag)
        {
            Gate(gateObjects[3], null, null, true, 2, 3, false, ref bothflag);
            ActiveObject(buttonObjects[1], null, cameraPointObjects[5], 2, 0, true, ref bothflag);
        }
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], null, cameraPointObjects[6], -1, 90, 2, 0, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
    // 
    public void Gimmick4()
    {
        if (enterArea[3].enterAreaFlag) Gate(gateObjects[2], lightObjects[3], cameraPointObjects[4], false, 2, 2, true, ref enterArea[3].enterAreaFlag);
        if (enemys[3].transform.childCount == 0 && defeatGateFlag[3])
        {
            ActiveLight(areaLightObjects[3], 2, 3, false, ref defeatGateFlag[3]);
            ActiveObject(buttonObjects[2], lightObjects[3], null, 2, 2, false, ref defeatGateFlag[3]);
            ActiveObject(buttonObjects[3], lightObjects[4], null, 2, 3, false, ref defeatGateFlag[3]);
            Gate(gateObjects[2], null, cameraPointObjects[8], true, 2, 4, true, ref defeatGateFlag[3]);
        }
        if (!defeatGateFlag[3] && buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], null, cameraPointObjects[6], -1, 90, 2, 0, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);
        if (!defeatGateFlag[3] && buttonObjects[3].GetComponent<ButtonManager>().buttonFlag) ActiveObject(goalObject, null, cameraPointObjects[9], 2, 4, true, ref buttonObjects[3].GetComponent<ButtonManager>().buttonFlag);
    }
}