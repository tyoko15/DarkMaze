public class GameManagerStage2to4 : GeneralStageManager
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
                OverUIControl();
                playerController.status = 4;
                break;
            case GameStatus.clear:
                playerController.status = 5;
                EndAnime();
                break;
        }
    }
    // “G‘SŒ‚”j‚Å‰ð•úƒMƒ~ƒbƒN
    public void Gimmick1()
    {
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(areaLightObjects[0], 2, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[0], lightObjects[0], cameraPointObjects[0], true, 2, 0, true, ref defeatGateFlag[0]);
        }
    }
    // 
    public void Gimmick2()
    {
        //if (enterArea[2].enterAreaFlag) PreGate(gateObjects[0], lightObjects[1], cameraPointObjects[1], false, 2, 2, true, ref enterArea[2].enterAreaFlag);
        if (enemys[2].transform.childCount == 0 && defeatGateFlag[2])
        {
            ActiveLight(areaLightObjects[2], 2, 2, false, ref defeatGateFlag[2]);
            Gate(gateObjects[1], lightObjects[1], null, true, 2, 1, false, ref defeatGateFlag[2]);
            Gate(gateObjects[2], lightObjects[2], cameraPointObjects[2], true, 2, 2, true, ref defeatGateFlag[2]);
        }
    }
    // 
    public void Gimmick3()
    {
        //if (enterArea[3].enterAreaFlag) PreGate(gateObjects[2], lightObjects[1], cameraPointObjects[3], false, 2, 2, true, ref enterArea[3].enterAreaFlag);
        if (enemys[3].transform.childCount == 0 && defeatGateFlag[3])
        {
            ActiveLight(areaLightObjects[3], 2, 3, false, ref defeatGateFlag[3]);
            Gate(gateObjects[2], lightObjects[2], null, true, 2, 2, false, ref defeatGateFlag[3]);
            Gate(gateObjects[3], null, null, true, 2, 3, false, ref defeatGateFlag[3]);
            Gate(gateObjects[4], null, null, true, 2, 4, false, ref defeatGateFlag[3]);
            Gate(gateObjects[5], lightObjects[2], cameraPointObjects[4], true, 2, 5, true, ref defeatGateFlag[3]);
        }
    }
    // 
    public void Gimmick4()
    {
        if (enterArea[1].enterAreaFlag)
        {
            Gate(gateObjects[4], null, null, false, 2, 4, false, ref enterArea[1].enterAreaFlag);
            Gate(gateObjects[5], lightObjects[2], cameraPointObjects[5], false, 2, 5, true, ref enterArea[1].enterAreaFlag);
        }
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            ActiveLight(areaLightObjects[1], 2, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[4], null, null, true, 2, 4, false, ref defeatGateFlag[1]);
            Gate(gateObjects[5], lightObjects[2], null, true, 2, 5, false, ref defeatGateFlag[1]);
            Gate(gateObjects[6], null, null, true, 2, 6, false, ref defeatGateFlag[1]);
            ActiveObject(buttonObjects[0], lightObjects[3], cameraPointObjects[6], 2, 0, true, ref defeatGateFlag[1]);
        }
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[1], null, cameraPointObjects[7], -1, 180, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }
}